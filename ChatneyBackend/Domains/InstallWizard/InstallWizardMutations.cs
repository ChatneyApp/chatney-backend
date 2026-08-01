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

    public async Task<InstallSystemResult> InstallSystem(
        AppConfig appConfig,
        AppRepos repos,
        IMigrationRunner migrationRunner
    )
    {
        try
        {
            migrationRunner.MigrateUp();

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

            var mainWorkspaceSecObjId = await SecureObjectHelper.Create(repos);
            var secondaryWorkspaceSecObjId = await SecureObjectHelper.Create(repos);

            List<Workspace> workspaces = new List<Workspace>
            {
                new() { Name = "Main", SecObjId = mainWorkspaceSecObjId },
                new() { Name = "Secondary", SecObjId = secondaryWorkspaceSecObjId },
            };
            await repos.Workspaces.InsertBulk(workspaces);

            var publicChannelTypeSecObjId = await SecureObjectHelper.Create(repos);
            var privateChannelTypeSecObjId = await SecureObjectHelper.Create(repos);

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

            var publicChannel1SecObjId = await SecureObjectHelper.Create(repos);
            var publicChannel2SecObjId = await SecureObjectHelper.Create(repos);
            var privateChannel1SecObjId = await SecureObjectHelper.Create(repos);
            var privateChannel2SecObjId = await SecureObjectHelper.Create(repos);

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

            List<Users.User> users = new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nickname = "test_user_1",
                    FullName = "Test User 1",
                    Email = "test1@test.com",
                    RoleId = adminRole.Id,
                    Password = Helpers.GetMd5Hash("123" + appConfig.UserPasswordSalt),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Nickname = "test_user_2",
                    FullName = "Test User 2",
                    Email = "test2@test.com",
                    RoleId = userRole.Id,
                    Password = Helpers.GetMd5Hash("123" + appConfig.UserPasswordSalt),
                },
            };
            await repos.Users.InsertBulk(users);

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

    public Task<InstallSystemResult> UnInstallSystem(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        IMigrationRunner migrationRunner)
    {
        try
        {
            while (migrationRunner.HasMigrationsToApplyRollback())
            {
                migrationRunner.Rollback(1);
            }
        }
        catch (Exception e)
        {
            return Task.FromResult(new InstallSystemResult()
            {
                status = "failed",
                message = e.ToString()
            });
        }

        return Task.FromResult(new InstallSystemResult()
        {
            status = "success"
        });
    }
}
