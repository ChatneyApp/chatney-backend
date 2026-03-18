using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using Dapper;
using Npgsql;

namespace ChatneyBackend.Domains.InstallWizard;

public class InstallWizardMutations
{
    public class InstallSystemResult
    {
        public required string status { get; set; }
        public string? message { get; set; }
    }

    public async Task<InstallSystemResult> InstallSystem(Repo<Role> roleRepo, NpgsqlDataSource pgDataSource)
    {
        // PG
        var pgConnection = await pgDataSource.OpenConnectionAsync();
        var sql = """
            CREATE TABLE IF NOT EXISTS roles (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                is_base bool NOT NULL DEFAULT false,
                permissions text[] NOT NULL DEFAULT '{}',
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );
        """;
        await pgConnection.ExecuteAsync(sql);
        await pgConnection.CloseAsync();

        // Mongo
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
