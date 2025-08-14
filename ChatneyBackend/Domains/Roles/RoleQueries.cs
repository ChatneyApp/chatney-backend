using MongoDB.Driver;

namespace ChatneyBackend.Domains.Roles;

public class RoleQueries
{
    public async Task<Role?> GetRoleById(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Role>("roles");
        var records = (await collection.FindAsync(r => r.Id == id)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    public async Task<Role?> GetRoleByName(IMongoDatabase mongoDatabase, string name)
    {
        var collection = mongoDatabase.GetCollection<Role>("roles");
        var records = (await collection.FindAsync(r => r.Name == name)).ToList();
        return records.Count > 0
            ? records[0]
            : null;

    }

    public async Task<List<Role>> GetList(IMongoDatabase mongoDatabase)
    {
        var collection = mongoDatabase.GetCollection<Role>("roles");
        var records = await collection.FindAsync(Builders<Role>.Filter.Empty);
        return records.ToList();

    }
}
