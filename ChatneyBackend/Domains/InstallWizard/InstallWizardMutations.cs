using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra;
using FluentMigrator.Runner;

namespace ChatneyBackend.Domains.InstallWizard;

public class InstallWizardMutations
{
    public class InstallSystemResult
    {
        public required string status { get; set; }
        public string? message { get; set; }
    }

    public async Task<InstallSystemResult> InstallSystem(PgRepo<Role, int> roleRepo, IMigrationRunner migrationRunner)
    {
        try
        {
            migrationRunner.MigrateUp();

            Role? baseRole = await roleRepo.GetOne(r => r.Name == Roles.DomainSettings.BaseRoleName);

            if (baseRole != null)
            {
                return new InstallSystemResult()
                {
                    status = "installed"
                };
            }

            await roleRepo.InsertOne(new Role
            {
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Name = Roles.DomainSettings.BaseRoleName,
                Permissions = [
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
            });
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
