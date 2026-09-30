using System.Security.Claims;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra;
using ChatneyBackend.Utils;
using FluentMigrator.Runner;
using Npgsql;

namespace ChatneyBackend.Domains.InstallWizard;

public class InstallWizardMutations
{
    public class InstallSystemResult
    {
        public required string status { get; set; }
        public string? message { get; set; }
    }

    public static readonly Permission[] UserWorkspacePermissions =
    [
        Permission.WorkspaceReadWorkspace,
        Permission.ChannelReadChannel,
        Permission.ChannelReadMessage,
        Permission.ChannelCreateMessage,
    ];

    public async Task<InstallSystemResult> InstallSystem(
        AppConfig appConfig,
        AppRepos repos,
        IMigrationRunner migrationRunner,
        NpgsqlDataSource dataSource
    )
    {
        try
        {
            migrationRunner.MigrateUp();

            // MapEnum registers the 'permission' enum lazily, but Npgsql only loads the
            // Postgres type catalog once, at the first physical connection open (before
            // migrations run on a fresh DB). Without reloading+clearing here, every later
            // read/write of permission[] fails with "Cannot resolve 'permission' to a
            // fully qualified datatype name" for the rest of the process lifetime.
            await dataSource.ReloadTypesAsync();
            dataSource.Clear();

            Role? adminRole = await repos.Roles.GetById(Roles.DomainSettings.AdminRoleId);

            if (adminRole != null)
            {
                return new InstallSystemResult()
                {
                    status = "installed"
                };
            }

            var now = DateTime.UtcNow;
            var allPermissions = Enum.GetValues<Permission>();

            // The admin role gets a fixed id so code can refer to it via AdminRoleId. It must exist
            // before any secure object is created (see SecureObjectHelper.Create) so every seeded
            // workspace/channel type/channel gets its admin role_acls row automatically.
            await repos.Roles.ExecuteAsync(
                """
                INSERT INTO roles (id, name, created_at, updated_at) OVERRIDING SYSTEM VALUE
                VALUES (@Id, @Name, @Now, @Now);

                SELECT setval(pg_get_serial_sequence('roles', 'id'), (SELECT MAX(id) FROM roles));
                """,
                new { Id = Roles.DomainSettings.AdminRoleId, Name = Roles.DomainSettings.AdminRoleName, Now = now });

            var userRole = new Role
            {
                Name = Roles.DomainSettings.UserRoleName,
                CreatedAt = now,
                UpdatedAt = now,
            };
            userRole.Id = await repos.Roles.InsertOne(userRole);

            var workspace = new Workspace
            {
                Name = "Default workspace",
                SecObjId = await SecureObjectHelper.Create(
                    repos, new SecureObjectDescription { Kind = "workspace", Name = "Default workspace" }),
            };
            workspace.Id = await repos.Workspaces.InsertOne(workspace);

            List<Channels.ChannelType> channelTypes = new List<Channels.ChannelType>
            {
                new()
                {
                    Name = "Public",
                    Key = "public",
                    SecObjId = await SecureObjectHelper.Create(
                        repos, new SecureObjectDescription { Kind = "channelType", Name = "Public" }),
                },
                new()
                {
                    Name = "Private",
                    Key = "private",
                    SecObjId = await SecureObjectHelper.Create(
                        repos, new SecureObjectDescription { Kind = "channelType", Name = "Private" }),
                },
            };
            await repos.ChannelTypes.InsertBulk(channelTypes);

            List<Channels.Channel> channels = new List<Channels.Channel>()
            {
                new()
                {
                    Name = "Public channel",
                    ChannelTypeId = channelTypes[0].Id,
                    WorkspaceId = workspace.Id,
                    SecObjId = await SecureObjectHelper.Create(
                        repos, new SecureObjectDescription { Kind = "channel", Name = "Public channel" }),
                },
                new()
                {
                    Name = "Private channel",
                    ChannelTypeId = channelTypes[1].Id,
                    WorkspaceId = workspace.Id,
                    SecObjId = await SecureObjectHelper.Create(
                        repos, new SecureObjectDescription { Kind = "channel", Name = "Private channel" }),
                },
            };
            await repos.Channels.InsertBulk(channels);

            // SecureObjectHelper.Create already granted admin the object-scoped permissions on the
            // workspace; widen that row to every permission explicitly.
            await repos.RoleAcls.Upsert(new RoleAcl
            {
                RoleId = Roles.DomainSettings.AdminRoleId,
                SecObjId = workspace.SecObjId,
                Permissions = allPermissions,
            });

            List<RoleAcl> roleAcls = new()
            {
                new()
                {
                    RoleId = Roles.DomainSettings.AdminRoleId,
                    SecObjId = SecureObjectIds.Global,
                    Permissions = allPermissions,
                },
                new()
                {
                    RoleId = userRole.Id,
                    SecObjId = workspace.SecObjId,
                    Permissions = InstallWizardMutations.UserWorkspacePermissions,
                },
            };
            await repos.RoleAcls.InsertBulk(roleAcls);

            var adminUser = new Users.User
            {
                Id = Guid.NewGuid(),
                Nickname = "admin",
                FullName = "Administrator",
                Email = "admin@chatney.local",
                Active = true,
                Verified = true,
                Password = Helpers.GetMd5Hash("admin" + appConfig.UserPasswordSalt),
                CreatedAt = now,
                UpdatedAt = now,
            };
            var testUsers = new List<Users.User>();

            for (var i = 1; i <= 2; i++)
            {
                testUsers.Add(new Users.User
                {
                    Id = Guid.NewGuid(),
                    Nickname = $"test{i}",
                    FullName = $"Test User {i}",
                    Email = $"test{i}@test.com",
                    Active = true,
                    Password = Helpers.GetMd5Hash("123" + appConfig.UserPasswordSalt),
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }

            await repos.Users.InsertBulk([adminUser, ..testUsers]);

            List<Users.UserRole> userRoles =
            [
                new() { UserId = adminUser.Id, RoleId = Roles.DomainSettings.AdminRoleId },
                ..testUsers.Select(user => new Users.UserRole { UserId = user.Id, RoleId = userRole.Id }),
            ];
            await repos.UserRoles.InsertBulk(userRoles);

            List<Configs.Config> configs = new()
            {
                new()
                {
                    Name = Configs.DomainSettings.SystemAdminUserId,
                    Value = adminUser.Id.ToString(),
                    Type = "string",
                },
                new()
                {
                    Name = Configs.DomainSettings.SystemDefaultUserRoleId,
                    Value = userRole.Id.ToString(),
                    Type = "int",
                },
                new()
                {
                    Name = Configs.DomainSettings.NewUserDefaultRole,
                    Value = userRole.Id.ToString(),
                    Type = "int",
                },
                new()
                {
                    Name = "messages.sendCooldown",
                    Value = "600",
                    Type = "int",
                },
                new()
                {
                    Name = "events.typesEnabled",
                    Value = "message.sent,message.edited,message.deleted",
                    Type = "string[]",
                },
            };
            await repos.Configs.InsertBulk(configs);

            appConfig.AdminUserId = adminUser.Id;
            appConfig.DefaultUserRoleId = userRole.Id;
        }
        catch (Exception e)
        {
            return new InstallSystemResult()
            {
                status = "failed",
                message = e.ToString()
            };
        }
        return new InstallSystemResult()
        {
            status = "success"
        };
    }

    public async Task<InstallSystemResult> UnInstallSystem(
        AppConfig appConfig,
        AppRepos repos,
        ClaimsPrincipal principal,
        IMigrationRunner migrationRunner,
        NpgsqlDataSource dataSource)
    {
        try
        {
            while (migrationRunner.HasMigrationsToApplyRollback())
            {
                migrationRunner.Rollback(1);
            }

            // Down() drops and a subsequent install recreates the 'permission' type with a
            // DIFFERENT OID. Without reloading+clearing here, the stale cached type info
            // would send wrong-OID binary traffic on the next install, failing obscurely.
            await dataSource.ReloadTypesAsync();
            dataSource.Clear();

            appConfig.AdminUserId = null;
            appConfig.DefaultUserRoleId = null;
        }
        catch (Exception e)
        {
            return new InstallSystemResult()
            {
                status = "failed",
                message = e.ToString()
            };
        }

        return new InstallSystemResult()
        {
            status = "success"
        };
    }
}
