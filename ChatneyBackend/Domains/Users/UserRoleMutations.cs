using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Users;

public class UserRoleMutations
{
    [Authorize]
    public async Task<UserRole> AddUserRole(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        UserRole userRole,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(UserPermissionNames.EditUser);

        var key = new UserRoleKey(
            userRole.UserId,
            userRole.ChannelId,
            userRole.ChannelTypeId,
            userRole.WorkspaceId);

        if (await repos.UserRoles.GetById(key) != null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        await repos.UserRoles.InsertOne(userRole);
        await webSocketConnector.SendNewUserRoleAsync(userRole);
        return userRole;
    }

    [Authorize]
    public async Task<UserRole> UpdateUserRole(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        UserRole userRole,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(UserPermissionNames.EditUser);

        var updated = await repos.UserRoles.UpdateOne(userRole);
        if (!updated)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        await webSocketConnector.SendUpdatedUserRoleAsync(userRole);
        return userRole;
    }

    [Authorize]
    public async Task<bool> DeleteUserRole(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        UserRoleKey key,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(UserPermissionNames.EditUser);

        var deleted = await repos.UserRoles.DeleteById(key);
        if (deleted)
        {
            await webSocketConnector.SendDeletedUserRoleAsync(WebsocketUserRoleDeletedPayload.FromKey(key));
        }

        return deleted;
    }
}
