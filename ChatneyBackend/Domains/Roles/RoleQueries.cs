using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Roles;

public class RoleQueries
{
    [Authorize]
    public async Task<Role?> GetRoleById(AppRepos repos, int id) =>
        await repos.Roles.GetById(id);

    [Authorize]
    public async Task<Role?> GetRoleByName(AppRepos repos, string name) =>
        await repos.Roles.GetOne(r => r.Name == name);

    [Authorize]
    public async Task<List<Role>> GetList(AppRepos repos) => await repos.Roles.GetList();
}
