using System.Diagnostics;
using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Permissions;

public class PermissionMutations
{
    public Permission AddPermission(ApplicationDbContext dbContext, Permission permission)
    {
        dbContext.Permissions.Add(permission);
        try
        {
            dbContext.SaveChanges();
            return permission;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return null;
    }

    public bool DeletePermission(ApplicationDbContext dbContext, string id)
    {
        var permission = dbContext.Permissions.First(permission => permission.Id == id);

        try
        {
            dbContext.Permissions.Remove(permission);
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
