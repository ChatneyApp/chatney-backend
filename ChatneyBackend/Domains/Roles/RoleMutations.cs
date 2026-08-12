using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Roles;

public class RoleMutations
{
    [Authorize]
    public async Task<Role> AddRole(
        AppRepos repos,
        IPermissionResolver resolver,
        RoleCreateDto roleDto,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.RoleCreateRole);

        var role = Role.FromDto(roleDto);
        role.Id = await repos.Roles.InsertOne(role);
        await webSocketConnector.SendNewRoleAsync(WebsocketRolePayload.FromRole(role));
        resolver.Invalidate();
        return role;
    }

    [Authorize]
    public async Task<Role> UpdateRole(
        AppRepos repos,
        IPermissionResolver resolver,
        RoleUpdateDto roleDto,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.RoleEditRole);

        var role = await repos.Roles.GetById(roleDto.Id);
        if (role == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        role.PatchFromDto(roleDto);

        await repos.Roles.UpdateOne(role);
        await webSocketConnector.SendUpdatedRoleAsync(WebsocketRolePayload.FromRole(role));
        resolver.Invalidate();
        return role;
    }

    [Authorize]
    public async Task<bool> DeleteRole(
        AppRepos repos,
        IPermissionResolver resolver,
        int id,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.RoleDeleteRole);

        var deleted = await repos.Roles.DeleteById(id);
        if (deleted)
        {
            await webSocketConnector.SendDeletedRoleAsync(new WebsocketRoleDeletedPayload { Id = id });
            resolver.Invalidate();
        }

        return deleted;
    }
}
