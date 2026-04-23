using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Roles;

public class RoleQueries
{
    public async Task<Role?> GetRoleById(AppRepos repos, int id) => await repos.Roles.GetById(id);

    public async Task<Role?> GetRoleByName(AppRepos repos, string name) => await repos.Roles.GetOne(r => r.Name == name);

    public async Task<List<Role>> GetList(AppRepos repos) => await repos.Roles.GetList();
}
