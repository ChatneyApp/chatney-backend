using ChatneyBackend.Models;

namespace ChatneyBackend.Domains.Workspaces;

[ExtendObjectType("Query")]
public class WorkspaceQueries
{
    public Workspace GetWorkspaceById(ApplicationDbContext dbContext, string id)
        => dbContext.Workspaces.First(w => w.Id == id);

    public Workspace GetWorkspaceByName(ApplicationDbContext dbContext, string name)
        => dbContext.Workspaces.First(w => w.Name == name);

    public IQueryable<Workspace> GetWorkspaces(ApplicationDbContext dbContext)
        => dbContext.Workspaces;
} 
