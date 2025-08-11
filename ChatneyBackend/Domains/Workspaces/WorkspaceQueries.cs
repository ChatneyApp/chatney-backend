using MongoDB.Driver;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceQueries
{
    public Workspace GetWorkspaceById(IMongoDatabase mongoDatabase, string id)
        => mongoDatabase.GetCollection<Workspace>("workspaces").Find(w => w.Id == id).First();

    public Workspace GetWorkspaceByName(IMongoDatabase mongoDatabase, string name)
        => mongoDatabase.GetCollection<Workspace>("workspaces").Find(w => w.Name == name).First();

    public List<Workspace> GetList(IMongoDatabase mongoDatabase)
        => mongoDatabase.GetCollection<Workspace>("workspaces").Find(Builders<Workspace>.Filter.Empty).ToList();
}
