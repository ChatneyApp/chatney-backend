using HotChocolate.Authorization;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceQueries
{
    [Authorize]
    public async Task<Workspace?> GetWorkspaceById(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Workspace>(DomainSettings.WorkspaceCollectionName);
        var records = (await collection.FindAsync(r => r.Id == id)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    [Authorize]
    public async Task<Workspace?> GetWorkspaceByName(IMongoDatabase mongoDatabase, string name)
    {
        var collection = mongoDatabase.GetCollection<Workspace>(DomainSettings.WorkspaceCollectionName);
        var records = (await collection.FindAsync(r => r.Name == name)).ToList();
        return records.Count > 0
            ? records[0]
            : null;

    }

    [Authorize]
    public async Task<List<Workspace>> GetList(IMongoDatabase mongoDatabase)
    {
        var collection = mongoDatabase.GetCollection<Workspace>(DomainSettings.WorkspaceCollectionName);
        var records = await collection.FindAsync(Builders<Workspace>.Filter.Empty);
        return records.ToList();
    }
}
