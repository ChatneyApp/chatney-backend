using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;

namespace ChatneyBackend.Domains.InstallWizard;

public class InstallWizardMutations
{
    public class InstallSystemResult
    {
        public required string status { get; set; }
        public string? message { get; set; }
    }

    public async Task<InstallSystemResult> InstallSystem(Repo<Role> roleRepo)
    {
        Role? baseRole = await roleRepo.GetOne((r) => r.Name == Roles.DomainSettings.BaseRoleName);

        if (baseRole != null)
        {
            return new InstallSystemResult()
            {
                status = "installed"
            };
        }

        try
        {

            await roleRepo.InsertOne(new Role
            {
                Id = Guid.NewGuid().ToString(),
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
                Settings = new RoleSettings()
                {
                    Base = true
                }
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
}