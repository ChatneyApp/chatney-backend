using System.Security.Claims;
using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using ChatneyBackend.Utils;
using FluentMigrator.Runner;
using HotChocolate.Authorization;
using Npgsql;

namespace ChatneyBackend.Domains.InstallWizard;

public class InstallWizardMutations
{
    public class InstallSystemResult
    {
        public required string status { get; set; }
        public string? message { get; set; }
    }

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

            Role? adminRole = await repos.Roles.GetOne(r => r.Name == Roles.DomainSettings.AdminRoleName);

            if (adminRole != null)
            {
                return new InstallSystemResult()
                {
                    status = "installed"
                };
            }

            var now = DateTime.UtcNow;
            var seedRoles = DefaultRoles.CreateSeedRoles(now);
            await repos.Roles.InsertBulk([..seedRoles]);

            var userRole = seedRoles.Single(r => r.Name == Roles.DomainSettings.UserRoleName);
            adminRole = seedRoles.Single(r => r.Name == Roles.DomainSettings.AdminRoleName);

            // The admin role must exist before any secure object is created (see
            // SecureObjectHelper.Create) so every seeded workspace/channel type/channel gets its
            // admin role_acls row automatically instead of being orphaned under D2.
            var mainWorkspaceSecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "workspace", Name = "Main" });
            var secondaryWorkspaceSecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "workspace", Name = "Secondary" });

            List<Workspace> workspaces = new List<Workspace>
            {
                new() { Name = "Main", SecObjId = mainWorkspaceSecObjId },
                new() { Name = "Secondary", SecObjId = secondaryWorkspaceSecObjId },
            };
            await repos.Workspaces.InsertBulk(workspaces);

            var publicChannelTypeSecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "channelType", Name = "public" });
            var privateChannelTypeSecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "channelType", Name = "private" });

            List<Channels.ChannelType> channelTypes = new List<Channels.ChannelType>
            {
                new()
                {
                    Name = "public",
                    Key = "public",
                    SecObjId = publicChannelTypeSecObjId,
                },
                new()
                {
                    Name = "private",
                    Key = "private",
                    SecObjId = privateChannelTypeSecObjId,
                },
            };
            await repos.ChannelTypes.InsertBulk(channelTypes);

            var publicChannel1SecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "channel", Name = "public 1" });
            var publicChannel2SecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "channel", Name = "public 2" });
            var privateChannel1SecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "channel", Name = "private 1" });
            var privateChannel2SecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "channel", Name = "private 2" });

            List<Channels.Channel> channels = new List<Channels.Channel>()
            {
                new()
                {
                    Name = "public 1",
                    ChannelTypeId = channelTypes[0].Id,
                    WorkspaceId = workspaces[0].Id,
                    SecObjId = publicChannel1SecObjId,
                },
                new()
                {
                    Name = "public 2",
                    ChannelTypeId = channelTypes[0].Id,
                    WorkspaceId = workspaces[0].Id,
                    SecObjId = publicChannel2SecObjId,
                },
                new()
                {
                    Name = "private 1",
                    ChannelTypeId = channelTypes[1].Id,
                    WorkspaceId = workspaces[0].Id,
                    SecObjId = privateChannel1SecObjId,
                },
                new()
                {
                    Name = "private 2",
                    ChannelTypeId = channelTypes[1].Id,
                    WorkspaceId = workspaces[0].Id,
                    SecObjId = privateChannel2SecObjId,
                },
            };
            await repos.Channels.InsertBulk(channels);

            var roleAcls = DefaultRoles.CreateSeedRoleAcls(seedRoles, mainWorkspaceSecObjId);
            await repos.RoleAcls.InsertBulk([..roleAcls]);

            List<Users.User> users = new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nickname = "test_user_1",
                    FullName = "Test User 1",
                    Email = "test1@test.com",
                    Password = Helpers.GetMd5Hash("123" + appConfig.UserPasswordSalt),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Nickname = "test_user_2",
                    FullName = "Test User 2",
                    Email = "test2@test.com",
                    Password = Helpers.GetMd5Hash("123" + appConfig.UserPasswordSalt),
                },
            };
            await repos.Users.InsertBulk(users);

            List<Users.UserRole> userRoles = new()
            {
                new() { UserId = users[0].Id, RoleId = adminRole.Id },
                new() { UserId = users[1].Id, RoleId = userRole.Id },
            };
            await repos.UserRoles.InsertBulk(userRoles);

            List<Configs.Config> configs = new()
            {
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
