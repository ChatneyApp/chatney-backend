using System.Security.Claims;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Roles;

public class RoleMutations
{
    [Authorize]
    public async Task<Role> AddRole(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        RoleDto roleDto)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.CreateRole);

        var role = Role.FromDto(roleDto);
        await repos.Roles.InsertOne(role);
        return role;
    }

    [Authorize]
    public async Task<Role> UpdateRole(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        Role role)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.EditRole);

        await repos.Roles.UpdateOne(role);
        return role;
    }

    [Authorize]
    public async Task<bool> DeleteRole(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.DeleteRole);

        return await repos.Roles.DeleteById(id);
    }
}
