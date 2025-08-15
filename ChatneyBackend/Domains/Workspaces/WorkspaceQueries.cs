using MongoDB.Driver;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceQueries
{
    public async Task<Workspace?> GetWorkspaceById(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Workspace>("workspaces");
        var records = (await collection.FindAsync(r => r.Id == id)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    public async Task<Workspace?> GetWorkspaceByName(IMongoDatabase mongoDatabase, string name)
    {
        var collection = mongoDatabase.GetCollection<Workspace>("workspaces");
        var records = (await collection.FindAsync(r => r.Name == name)).ToList();
        return records.Count > 0
            ? records[0]
            : null;

    }

    public async Task<List<Workspace>> GetList(IMongoDatabase mongoDatabase)
    {
        var collection = mongoDatabase.GetCollection<Workspace>("workspaces");
        var records = await collection.FindAsync(Builders<Workspace>.Filter.Empty);
        return records.ToList();
    }
}
