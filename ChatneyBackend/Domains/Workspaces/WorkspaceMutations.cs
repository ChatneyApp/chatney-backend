using MongoDB.Driver;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceMutations
{
    public async Task<Workspace> AddWorkspace(IMongoDatabase mongoDatabase, WorkspaceDTO workspaceDto)
    {
        var collection = mongoDatabase.GetCollection<Workspace>(DomainSettings.WorkspaceCollectionName);
        Workspace workspace = Workspace.FromDTO(workspaceDto);
        await collection.InsertOneAsync(workspace);
        return workspace;
    }

    public async Task<Workspace?> UpdateWorkspace(IMongoDatabase mongoDatabase, Workspace workspace)
    {
        var collection = mongoDatabase.GetCollection<Workspace>(DomainSettings.WorkspaceCollectionName);
        var filter = Builders<Workspace>.Filter.Eq("_id", workspace.Id);
        var result = await collection.ReplaceOneAsync(filter, workspace);
        return result.ModifiedCount > 0 ? workspace : null;
    }

    public async Task<bool> DeleteWorkspace(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Workspace>(DomainSettings.WorkspaceCollectionName);
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
