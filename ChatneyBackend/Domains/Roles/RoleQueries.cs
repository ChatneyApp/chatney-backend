using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Roles;

public class RoleQueries
{
    /// <summary>A role by id, or null if it doesn't exist.</summary>
    [Authorize]
    public async Task<Role?> GetRoleById(AppRepos repos, int id) =>
        await repos.Roles.GetById(id);

    /// <summary>A role by exact name, or null if it doesn't exist.</summary>
    [Authorize]
    public async Task<Role?> GetRoleByName(AppRepos repos, string name) =>
        await repos.Roles.GetOne(r => r.Name == name);

    /// <summary>All roles.</summary>
    [Authorize]
    public async Task<List<Role>> GetList(AppRepos repos) => await repos.Roles.GetList();
}
