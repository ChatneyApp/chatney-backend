using ChatneyBackend.Models;

namespace ChatneyBackend.Domains.Permissions;

[ExtendObjectType("Query")]
public class PermissionQueries
{
    public Permission GetPermissionById(ApplicationDbContext dbContext, string id)
        => dbContext.Permissions.First(p => p.Id == id);

    public Permission GetPermissionByName(ApplicationDbContext dbContext, string name)
        => dbContext.Permissions.First(p => p.Name == name);

    public IQueryable<Permission> GetPermissions(ApplicationDbContext dbContext)
        => dbContext.Permissions;
} 
