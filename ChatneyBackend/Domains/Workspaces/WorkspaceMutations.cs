using MongoDB.Driver;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceMutations
{
    public Workspace AddWorkspace(IMongoDatabase mongoDatabase, Workspace workspace)
    {
        var collection = mongoDatabase.GetCollection<Workspace>("workspaces");
        collection.InsertOne(workspace);
        return workspace;
    }

    public bool DeleteWorkspace(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Workspace>("workspaces");
        var result = collection.DeleteOne(w => w.Id == id);
        return result.DeletedCount > 0;
    }
}
