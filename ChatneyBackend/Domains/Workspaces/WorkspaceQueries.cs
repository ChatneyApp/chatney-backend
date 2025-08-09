using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceQueries
{
    public Workspace GetWorkspaceById(ApplicationDbContext dbContext, string id)
        => dbContext.Workspaces.First(w => w.Id == id);

    public Workspace GetWorkspaceByName(ApplicationDbContext dbContext, string name)
        => dbContext.Workspaces.First(w => w.Name == name);

    public IQueryable<Workspace> GetList(ApplicationDbContext dbContext)
        => dbContext.Workspaces;
} 
