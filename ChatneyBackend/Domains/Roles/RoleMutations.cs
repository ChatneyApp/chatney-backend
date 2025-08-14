using MongoDB.Driver;

namespace ChatneyBackend.Domains.Roles;

public class RoleMutations
{
    public async Task<Role> AddRole(IMongoDatabase mongoDatabase, RoleDTO roleDto)
    {
        var collection = mongoDatabase.GetCollection<Role>("roles");
        Role role = Role.FromDTO(roleDto);
        await collection.InsertOneAsync(role);
        return role;
    }

    public async Task<Role?> UpdateRole(IMongoDatabase mongoDatabase, Role role)
    {
        var collection = mongoDatabase.GetCollection<Role>("roles");
        var filter = Builders<Role>.Filter.Eq("_id", role.Id);
        var result = await collection.ReplaceOneAsync(filter, role);
        return result.ModifiedCount > 0 ? role : null;
    }

    public async Task<bool> DeleteRole(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Role>("roles");
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
