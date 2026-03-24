using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Roles;

public class RoleQueries
{
    public async Task<Role?> GetRoleById(PgRepo<Role, int> rolesRepo, int id) => await rolesRepo.GetById(id);

    public async Task<Role?> GetRoleByName(PgRepo<Role, int> rolesRepo, string name) => await rolesRepo.GetOne($"name = @name", new { name });

    public async Task<List<Role>> GetList(PgRepo<Role, int> rolesRepo) => await rolesRepo.GetList();
}
