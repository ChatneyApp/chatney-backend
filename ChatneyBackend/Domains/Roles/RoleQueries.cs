using MongoDB.Driver;

namespace ChatneyBackend.Domains.Roles;

public class RoleQueries
{
    public Role GetRoleById(IMongoDatabase mongoDatabase, string id)
        => mongoDatabase.GetCollection<Role>("roles").Find(r => r.Id == id).First();

    public Role GetRoleByName(IMongoDatabase mongoDatabase, string name)
        => mongoDatabase.GetCollection<Role>("roles").Find(r => r.Name == name).First();

    public List<Role> GetList(IMongoDatabase mongoDatabase)
        => mongoDatabase
            .GetCollection<Role>("roles")
            .Find(Builders<Role>.Filter.Empty)
            .ToList();
}
