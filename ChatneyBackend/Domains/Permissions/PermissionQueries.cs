using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Permissions;

public class PermissionQueries
{
    public IQueryable<Permission> GetList(ApplicationDbContext dbContext)
        => dbContext.Permissions;
} 
