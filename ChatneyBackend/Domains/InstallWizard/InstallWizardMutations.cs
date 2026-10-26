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
    public class SmtpSettingsInput
    {
        public required string Host { get; set; }
        public int Port { get; set; } = 587;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool EnableSsl { get; set; } = true;
        public required string FromAddress { get; set; }
    }

    public class InstallSystemInput
    {
        public required string DomainName { get; set; }
        public required string TimeZone { get; set; }
        public bool SeedTestData { get; set; }
        public SmtpSettingsInput? Smtp { get; set; }
    }

    /// <summary>Outcome of an install/uninstall run.</summary>
    public class InstallSystemResult
    {
        /// <summary>"success", "installed" (already installed, nothing done) or "failed".</summary>
        public required string status { get; set; }
        /// <summary>Error details when status is "failed".</summary>
        public string? message { get; set; }
    }

    public static readonly Permission[] UserWorkspacePermissions =
    [
        Permission.WorkspaceReadWorkspace,
        Permission.WorkspaceReadChannelGroup,
    ];

    public static readonly Permission[] UserPublicChannelTypePermissions =
    [
        Permission.ChannelReadMessage,
        Permission.ChannelReadChannel,
        Permission.ChannelCreateMessage,
        Permission.ChannelDeleteOwnMessage,
        Permission.ChannelEditOwnMessage,
        Permission.AttachmentDelete,
        Permission.AttachmentRead,
        Permission.AttachmentUpload,
        Permission.ChannelAddReaction,
        Permission.ChannelRemoveReaction,
    ];

    public static readonly Permission[] PrivateUserChannelTypePermissions =
    [
        Permission.AttachmentDelete,
        Permission.AttachmentUpload,
        Permission.AttachmentRead,
        Permission.ChannelCreateMessage,
        Permission.ChannelDeleteOwnMessage,
        Permission.ChannelEditOwnMessage,
        Permission.ChannelReadMessage,
        Permission.ChannelReadChannel,
        Permission.ChannelAddReaction,
        Permission.ChannelRemoveReaction,
    ];

    public static readonly Permission[] UserReadonlyChannelTypePermissions =
    [
        Permission.ChannelReadMessage,
        Permission.ChannelReadChannel,
        Permission.ChannelAddReaction,
        Permission.ChannelRemoveReaction,
    ];

    public static readonly Permission[] UserDryTextChannelTypePermissions =
    [
        Permission.ChannelCreateMessage,
        Permission.ChannelDeleteOwnMessage,
        Permission.ChannelEditOwnMessage,
        Permission.ChannelReadMessage,
        Permission.ChannelReadChannel,
    ];

    /// <summary>
    /// Runs migrations and seeds a fresh system: admin and user roles, a default workspace with
    /// public/private/readonly channel types, channels and channel groups, an admin user, optional
    /// test users and default config. Safe to call again: returns status "installed" if the system
    /// is already installed.
    /// </summary>
    public async Task<InstallSystemResult> InstallSystem(
        InstallSystemInput options,
        AppConfig appConfig,
        AppRepos repos,
        IMigrationRunner migrationRunner,
        NpgsqlDataSource dataSource
    )
    {
        try
        {
            ValidateOptions(options);
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
                await MarkInstalled(repos);

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

            var privateUserRole = new Role
            {
                Name = "Private user",
                CreatedAt = now,
                UpdatedAt = now,
            };
            privateUserRole.Id = await repos.Roles.InsertOne(privateUserRole);

            var workspace = new Workspace
            {
                Name = "Default workspace",
                SecObjId = await SecureObjectHelper.Create(
                    repos, new SecureObjectDescription { Kind = "workspace", Name = "Default workspace" }),
            };
            workspace.Id = await repos.Workspaces.InsertOne(workspace);

            var publicType = await NewChannelType(repos, "Public", "public");
            var privateType = await NewChannelType(repos, "Private", "private");
            var readonlyType = await NewChannelType(repos, "Readonly", "readonly");
            var dryTextType = await NewChannelType(repos, "Dry text", "dry-text");
            await repos.ChannelTypes.InsertBulk([publicType, privateType, readonlyType, dryTextType]);

            var public1 = await NewChannel(repos, "Public 1", publicType.Id, workspace.Id);
            var private1 = await NewChannel(repos, "Private 1", privateType.Id, workspace.Id);
            var public2 = await NewChannel(repos, "Public 2", publicType.Id, workspace.Id);
            var public3 = await NewChannel(repos, "Public 3", publicType.Id, workspace.Id);
            var readonlyChannel = await NewChannel(repos, "Readonly", readonlyType.Id, workspace.Id);
            var private2 = await NewChannel(repos, "Private 2", privateType.Id, workspace.Id);
            var dryText = await NewChannel(repos, "Dry text", dryTextType.Id, workspace.Id);
            await repos.Channels.InsertBulk([public1, private1, public2, public3, readonlyChannel, private2, dryText]);

            await repos.ChannelGroups.InsertBulk(
            [
                new()
                {
                    Name = "Public space",
                    WorkspaceId = workspace.Id,
                    ChannelIds = [public1.Id, public2.Id, public3.Id],
                    Order = 0,
                },
                new()
                {
                    Name = "Private space",
                    WorkspaceId = workspace.Id,
                    ChannelIds = [private1.Id, private2.Id],
                    Order = 1,
                },
                new()
                {
                    Name = "Rules",
                    WorkspaceId = workspace.Id,
                    ChannelIds = [readonlyChannel.Id],
                    Order = 2,
                },
                new()
                {
                    Name = "Dry text",
                    WorkspaceId = workspace.Id,
                    ChannelIds = [dryText.Id],
                    Order = 3,
                },
            ]);

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
                    Permissions = UserWorkspacePermissions,
                },
                new()
                {
                    RoleId = userRole.Id,
                    SecObjId = publicType.SecObjId,
                    Permissions = UserPublicChannelTypePermissions,
                },
                new()
                {
                    RoleId = userRole.Id,
                    SecObjId = readonlyType.SecObjId,
                    Permissions = UserReadonlyChannelTypePermissions,
                },
                new()
                {
                    RoleId = userRole.Id,
                    SecObjId = dryTextType.SecObjId,
                    Permissions = UserDryTextChannelTypePermissions,
                },
                new()
                {
                    RoleId = privateUserRole.Id,
                    SecObjId = privateType.SecObjId,
                    Permissions = PrivateUserChannelTypePermissions,
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

            if (options.SeedTestData)
            {
                for (var i = 1; i <= 2; i++)
                {
                    testUsers.Add(new Users.User
                    {
                        Id = Guid.NewGuid(),
                        Nickname = $"test{i}",
                        FullName = $"Test User {i}",
                        Email = $"test{i}@test.com",
                        Active = true,
                        Verified = true,
                        Muted = false,
                        Password = Helpers.GetMd5Hash("123" + appConfig.UserPasswordSalt),
                        CreatedAt = now,
                        UpdatedAt = now,
                    });
                }
            }

            await repos.Users.InsertBulk([adminUser, ..testUsers]);

            List<Users.UserRole> userRoles =
            [
                new() { UserId = adminUser.Id, RoleId = Roles.DomainSettings.AdminRoleId },
                ..testUsers.Select(user => new Users.UserRole { UserId = user.Id, RoleId = userRole.Id }),
                ..testUsers.Skip(1).Take(1).Select(user => new Users.UserRole { UserId = user.Id, RoleId = privateUserRole.Id }),
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
                    Name = Configs.DomainSettings.SystemDomainName,
                    Value = options.DomainName.TrimEnd('/'),
                    Type = "string",
                },
                new()
                {
                    Name = Configs.DomainSettings.SystemTimeZone,
                    Value = options.TimeZone,
                    Type = "string",
                },
                new()
                {
                    Name = Configs.DomainSettings.SmtpEnabled,
                    Value = (options.Smtp != null).ToString().ToLowerInvariant(),
                    Type = "bool",
                },
                new()
                {
                    Name = Configs.DomainSettings.SystemInstalled,
                    Value = "true",
                    Type = "bool",
                },
            };

            if (options.Smtp != null)
            {
                configs.AddRange(
                [
                    new()
                    {
                        Name = Configs.DomainSettings.SmtpHost,
                        Value = options.Smtp.Host,
                        Type = "string",
                    },
                    new()
                    {
                        Name = Configs.DomainSettings.SmtpPort,
                        Value = options.Smtp.Port.ToString(),
                        Type = "int",
                    },
                    new()
                    {
                        Name = Configs.DomainSettings.SmtpUsername,
                        Value = options.Smtp.Username ?? "",
                        Type = "string",
                    },
                    new()
                    {
                        Name = Configs.DomainSettings.SmtpPassword,
                        Value = options.Smtp.Password ?? "",
                        Type = "string",
                    },
                    new()
                    {
                        Name = Configs.DomainSettings.SmtpEnableSsl,
                        Value = options.Smtp.EnableSsl.ToString().ToLowerInvariant(),
                        Type = "bool",
                    },
                    new()
                    {
                        Name = Configs.DomainSettings.SmtpFromAddress,
                        Value = options.Smtp.FromAddress,
                        Type = "string",
                    },
                ]);
            }

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

    private static async Task<Channels.ChannelType> NewChannelType(AppRepos repos, string name, string key)
    {
        return new Channels.ChannelType
        {
            Name = name,
            Key = key,
            SecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "channelType", Name = name }),
        };
    }

    private static async Task<Channels.Channel> NewChannel(AppRepos repos, string name, int channelTypeId, int workspaceId)
    {
        return new Channels.Channel
        {
            Name = name,
            ChannelTypeId = channelTypeId,
            WorkspaceId = workspaceId,
            SecObjId = await SecureObjectHelper.Create(
                repos, new SecureObjectDescription { Kind = "channel", Name = name }),
        };
    }

    private static async Task MarkInstalled(AppRepos repos)
    {
        var installed = await repos.Configs.GetOne(
            config => config.Name == Configs.DomainSettings.SystemInstalled);

        if (installed == null)
        {
            await repos.Configs.InsertOne(new Configs.Config
            {
                Name = Configs.DomainSettings.SystemInstalled,
                Value = "true",
                Type = "bool",
            });

            return;
        }

        installed.Value = "true";
        installed.Type = "bool";
        await repos.Configs.UpdateOne(installed);
    }

    private static void ValidateOptions(InstallSystemInput options)
    {
        if (!Uri.TryCreate(options.DomainName, UriKind.Absolute, out var domain)
            || (domain.Scheme != Uri.UriSchemeHttp && domain.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Domain name must be an absolute HTTP or HTTPS URL.");
        }

        if (string.IsNullOrWhiteSpace(options.TimeZone))
        {
            throw new ArgumentException("Time zone is required.");
        }

        if (options.Smtp != null
            && (string.IsNullOrWhiteSpace(options.Smtp.Host)
                || options.Smtp.Port is < 1 or > 65535
                || string.IsNullOrWhiteSpace(options.Smtp.FromAddress)))
        {
            throw new ArgumentException("SMTP host, port and from address are required.");
        }
    }

    /// <summary>Rolls back every migration, deleting ALL data. Development use only.</summary>
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
