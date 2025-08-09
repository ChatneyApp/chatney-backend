using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Roles;

public class RoleQueries
{
    public Role GetRoleById(ApplicationDbContext dbContext, string id)
        => dbContext.Roles.First(r => r.Id == id);

    public Role GetRoleByName(ApplicationDbContext dbContext, string name)
        => dbContext.Roles.First(r => r.Name == name);

    public IQueryable<Role> GetList(ApplicationDbContext dbContext)
        => dbContext.Roles;
} 
