using MongoDB.Driver;

namespace ChatneyBackend.Domains.Roles;

public class RoleMutations
{
    public Role AddRole(IMongoDatabase mongoDatabase, Role role)
    {
        var collection = mongoDatabase.GetCollection<Role>("roles");
        collection.InsertOne(role);
        return role;
    }

    public bool DeleteRole(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Role>("roles");
        var result = collection.DeleteOne(r => r.Id == id);
        return result.DeletedCount > 0;
    }
}
