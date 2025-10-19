using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Configs;

public class InstallSystemResult
{
    public string status { get; set; }
    public string? message {get; set;}
}


public class ConfigMutations
{
    // TODO: ignore type field - it's constant for system config
    public async Task<Config?> UpdateConfig(IMongoDatabase mongoDatabase, Config config)
    {
        var collection = mongoDatabase.GetCollection<Config>(DomainSettings.ConfigCollectionName);
        var filter = Builders<Config>.Filter.Eq("_id", config.Id);
        var result = await collection.ReplaceOneAsync(filter, config);
        return result.ModifiedCount > 0 ? config : null;
    }

    public async Task<InstallSystemResult> InstallSystem(Repo<Role> roleRepo)
    {
        Role? baseRole = await roleRepo.GetOne((r) => r.Name == Domains.Configs.DomainSettings.BaseRoleName);

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
                UpdatedAt = new DateTime(),
                CreatedAt = new DateTime(),
                Name = DomainSettings.BaseRoleName,
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
