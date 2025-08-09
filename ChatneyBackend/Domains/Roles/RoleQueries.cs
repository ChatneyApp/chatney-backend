using ChatneyBackend.Models;

namespace ChatneyBackend.Domains.Roles;

[ExtendObjectType("Query")]
public class RoleQueries
{
    public Role GetRoleById(ApplicationDbContext dbContext, string id)
        => dbContext.Roles.First(r => r.Id == id);

    public Role GetRoleByName(ApplicationDbContext dbContext, string name)
        => dbContext.Roles.First(r => r.Name == name);

    public IQueryable<Role> GetRoles(ApplicationDbContext dbContext)
        => dbContext.Roles;
} 
