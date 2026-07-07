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
        RoleCreateDto roleDto,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.CreateRole);

        var role = Role.FromDto(roleDto);
        role.Id = await repos.Roles.InsertOne(role);
        await webSocketConnector.SendNewRoleAsync(WebsocketRolePayload.FromRole(role));
        return role;
    }

    [Authorize]
    public async Task<Role> UpdateRole(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        RoleUpdateDto roleDto,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.EditRole);

        var role = await repos.Roles.GetById(roleDto.Id);
        if (role == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        role.PatchFromDto(roleDto);

        await repos.Roles.UpdateOne(role);
        await webSocketConnector.SendUpdatedRoleAsync(WebsocketRolePayload.FromRole(role));
        return role;
    }

    [Authorize]
    public async Task<bool> DeleteRole(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.DeleteRole);

        var deleted = await repos.Roles.DeleteById(id);
        if (deleted)
        {
            await webSocketConnector.SendDeletedRoleAsync(new WebsocketRoleDeletedPayload { Id = id });
        }

        return deleted;
    }
}
