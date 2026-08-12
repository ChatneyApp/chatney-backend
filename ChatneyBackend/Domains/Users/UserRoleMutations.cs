using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Users;

public class UserRoleMutations
{
    [Authorize]
    public async Task<UserRole> AssignRole(
        AppRepos repos,
        IPermissionResolver resolver,
        Guid userId,
        int roleId,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.UserEditUser);

        var key = new UserRoleKey(userId, roleId);
        var existing = await repos.UserRoles.GetById(key);

        if (existing != null)
        {
            return existing;
        }

        var userRole = new UserRole { UserId = userId, RoleId = roleId };
        await repos.UserRoles.InsertOne(userRole);
        await webSocketConnector.SendNewUserRoleAsync(userRole);
        resolver.Invalidate();
        return userRole;
    }

    [Authorize]
    public async Task<bool> UnassignRole(
        AppRepos repos,
        IPermissionResolver resolver,
        Guid userId,
        int roleId,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.UserEditUser);

        var key = new UserRoleKey(userId, roleId);
        var deleted = await repos.UserRoles.DeleteById(key);

        if (deleted)
        {
            await webSocketConnector.SendDeletedUserRoleAsync(WebsocketUserRoleDeletedPayload.FromKey(key));
        }

        resolver.Invalidate();
        return deleted;
    }
}
