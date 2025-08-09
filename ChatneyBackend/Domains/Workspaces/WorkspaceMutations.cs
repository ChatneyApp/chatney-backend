using System.Diagnostics;
using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceMutations
{
    public Workspace AddWorkspace(ApplicationDbContext dbContext, Workspace workspace)
    {
        dbContext.Workspaces.Add(workspace);
        try
        {
            dbContext.SaveChanges();
            return workspace;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return null;
    }

    public bool DeleteWorkspace(ApplicationDbContext dbContext, string id)
    {
        var workspace = dbContext.Workspaces.First(workspace => workspace.Id == id);

        try
        {
            dbContext.Workspaces.Remove(workspace);
            dbContext.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return false;
    }
} 
