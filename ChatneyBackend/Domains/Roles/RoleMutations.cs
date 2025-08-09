using System.Diagnostics;
using ChatneyBackend.Models;

namespace ChatneyBackend.Domains.Roles;

[ExtendObjectType("Mutation")]
public class RoleMutations
{
    public Role AddRole(ApplicationDbContext dbContext, Role role)
    {
        dbContext.Roles.Add(role);
        try
        {
            dbContext.SaveChanges();
            return role;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return null;
    }

    public bool DeleteRole(ApplicationDbContext dbContext, string id)
    {
        var role = dbContext.Roles.First(role => role.Id == id);

        try
        {
            dbContext.Roles.Remove(role);
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