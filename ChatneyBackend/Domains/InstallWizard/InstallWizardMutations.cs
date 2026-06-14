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

namespace ChatneyBackend.Domains.InstallWizard;

public class InstallWizardMutations
{
    public class InstallSystemResult
    {
        public required string status { get; set; }
        public string? message { get; set; }
    }

    public static string[] BaseRolePermissions =>
    [
        UserPermissionNames.ReadUser,
        UserPermissionNames.EditUser,
        ChannelPermissions.CreateMessage,
        ChannelPermissions.DeleteMessage,
        ChannelPermissions.EditMessage,
        ChannelPermissions.ReadChannel,
        ChannelPermissions.ReadMessage,
        ChannelPermissions.EditOwnMessage,
        ChannelPermissions.DeleteOwnMessage,
        WorkspacePermissions.ReadWorkspace,
        AttachmentPermissions.Upload,
        AttachmentPermissions.Read,
    ];

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
                Permissions = BaseRolePermissions,
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

    [Authorize]
    public async Task<InstallSystemResult> UnInstallSystem(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        IMigrationRunner migrationRunner)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(SystemConfigPermissions.UpdateValue);

        migrationRunner.MigrateDown(0);

        return new InstallSystemResult()
        {
            status = "success"
        };
    }
}
