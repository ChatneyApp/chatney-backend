using System.Security.Claims;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Roles;

public class RoleQueries
{
    [Authorize]
    public async Task<Role?> GetRoleById(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.ReadRole);

        return await repos.Roles.GetById(id);
    }

    [Authorize]
    public async Task<Role?> GetRoleByName(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        string name)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.ReadRole);

        return await repos.Roles.GetOne(r => r.Name == name);
    }

    [Authorize]
    public async Task<List<Role>> GetList(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.ReadRole);

        return await repos.Roles.GetList();
    }
}
