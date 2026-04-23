using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra;
using ChatneyBackend.Utils;
using FluentMigrator.Runner;

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
        IMigrationRunner migrationRunner
    )
    {
        try
        {
            migrationRunner.MigrateUp();

            Role? baseRole = await repos.Roles.GetOne(r => r.Name == Roles.DomainSettings.BaseRoleName);

            if (baseRole != null)
            {
                return new InstallSystemResult()
                {
                    status = "installed"
                };
            }

            baseRole = new()
            {
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Name = Roles.DomainSettings.BaseRoleName,
                Permissions =
                [
                    MessagePermissions.CreateMessage,
                    MessagePermissions.DeleteMessage,
                    MessagePermissions.EditMessage,
                    MessagePermissions.ReadMessage,
                    UserPermissions.ReadUser,
                    UserPermissions.EditUser,
                    ChannelPermissions.CreateMessage,
                    ChannelPermissions.DeleteMessage,
                    ChannelPermissions.EditMessage,
                    ChannelPermissions.ReadChannel,
                    ChannelPermissions.ReadMessage,
                    WorkspacePermissions.ReadWorkspace
                ],
                IsBase = true
            };

            await repos.Roles.InsertOne(baseRole);

            List<Workspace> workspaces = new List<Workspace>
            {
                new() { Name = "Main" },
                new() { Name = "Secondary" },
            };
            await repos.Workspaces.InsertBulk(workspaces);

            List<Channels.ChannelType> channelTypes = new List<Channels.ChannelType>
            {
                new()
                {
                    Name = "public",
                    Key = "public",
                    BaseRoleId = baseRole.Id
                },
                new()
                {
                    Name = "private",
                    Key = "private",
                    BaseRoleId = baseRole.Id
                },
            };
            await repos.ChannelTypes.InsertBulk(channelTypes);

            List<Channels.Channel> channels = new List<Channels.Channel>()
            {
                new()
                {
                    Name = "public 1",
                    ChannelTypeId = channelTypes[0].Id,
                    WorkspaceId = workspaces[0].Id,
                },
                new()
                {
                    Name = "public 2",
                    ChannelTypeId = channelTypes[0].Id,
                    WorkspaceId = workspaces[0].Id,
                },
                new()
                {
                    Name = "private 1",
                    ChannelTypeId = channelTypes[1].Id,
                    WorkspaceId = workspaces[0].Id,
                },
                new()
                {
                    Name = "private 2",
                    ChannelTypeId = channelTypes[1].Id,
                    WorkspaceId = workspaces[0].Id,
                },
            };
            await repos.Channels.InsertBulk(channels);

            var workspaceIds = workspaces.Select(w => w.Id).ToArray();
            List<Users.User> users = new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "test user 1",
                    Email = "test1@test.com",
                    RoleId = baseRole.Id,
                    WorkspaceIds = workspaceIds,
                    Password = Helpers.GetMd5Hash("123" + appConfig.UserPasswordSalt),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "test user 2",
                    Email = "test2@test.com",
                    RoleId = baseRole.Id,
                    WorkspaceIds = workspaceIds,
                    Password = Helpers.GetMd5Hash("123" + appConfig.UserPasswordSalt),
                },
            };
            await repos.Users.InsertBulk(users);

            List<Configs.Config> configs = new()
            {
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

    public async Task<InstallSystemResult> UnInstallSystem(IMigrationRunner migrationRunner)
    {
        migrationRunner.MigrateDown(0);

        return new InstallSystemResult()
        {
            status = "success"
        };
    }
}
