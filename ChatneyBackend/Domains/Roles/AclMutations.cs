using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Roles;

public class AclMutations
{
    /// <summary>
    /// An empty permission list DELETES the row rather than storing an empty array, mirroring
    /// SetUserAcl - a present-but-empty row is dead weight that would still surface in
    /// roleAcls/objectAcls and confuse the admin UI. See AclMutationsTests for the guard.
    /// </summary>
    [Authorize]
    public async Task<RoleAcl?> SetRoleAcl(
        AppRepos repos,
        IPermissionResolver resolver,
        WebSocketConnector webSocketConnector,
        int roleId,
        int secObjId,
        Permission[] permissions)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.RoleEditRole);

        if (permissions.Length == 0)
        {
            await DeleteRoleAclCore(repos, webSocketConnector, roleId, secObjId);
            resolver.Invalidate();
            return null;
        }

        var roleAcl = new RoleAcl { RoleId = roleId, SecObjId = secObjId, Permissions = permissions };
        await repos.RoleAcls.Upsert(roleAcl);
        await webSocketConnector.SendRoleAclChangedAsync(WebsocketRoleAclPayload.FromRoleAcl(roleAcl));
        resolver.Invalidate();
        return roleAcl;
    }

    [Authorize]
    public async Task<bool> DeleteRoleAcl(
        AppRepos repos,
        IPermissionResolver resolver,
        WebSocketConnector webSocketConnector,
        int roleId,
        int secObjId)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.RoleEditRole);

        var deleted = await DeleteRoleAclCore(repos, webSocketConnector, roleId, secObjId);

        if (deleted)
        {
            resolver.Invalidate();
        }

        return deleted;
    }

    private static async Task<bool> DeleteRoleAclCore(
        AppRepos repos, WebSocketConnector webSocketConnector, int roleId, int secObjId)
    {
        var deleted = await repos.RoleAcls.DeleteById(new RoleAclKey(roleId, secObjId));

        if (deleted)
        {
            await webSocketConnector.SendRoleAclDeletedAsync(
                new WebsocketRoleAclDeletedPayload { RoleId = roleId, SecObjId = secObjId });
        }

        return deleted;
    }

    /// <summary>
    /// An empty permission list DELETES the row rather than storing an empty array - a present-but-
    /// empty user_acls row would still satisfy D1's "any user_acls row in the chain" switch and
    /// would silently suppress every role permission for that chain. See AclMutationsTests for the
    /// guard.
    /// </summary>
    [Authorize]
    public async Task<UserAcl?> SetUserAcl(
        AppRepos repos,
        IPermissionResolver resolver,
        WebSocketConnector webSocketConnector,
        Guid userId,
        int secObjId,
        Permission[] permissions)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.UserEditUser);

        if (permissions.Length == 0)
        {
            await DeleteUserAclCore(repos, webSocketConnector, userId, secObjId);
            resolver.Invalidate();
            return null;
        }

        var userAcl = new UserAcl { UserId = userId, SecObjId = secObjId, Permissions = permissions };
        await repos.UserAcls.Upsert(userAcl);
        await webSocketConnector.SendUserAclChangedAsync(WebsocketUserAclPayload.FromUserAcl(userAcl));
        resolver.Invalidate();
        return userAcl;
    }

    [Authorize]
    public async Task<bool> DeleteUserAcl(
        AppRepos repos,
        IPermissionResolver resolver,
        WebSocketConnector webSocketConnector,
        Guid userId,
        int secObjId)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.UserEditUser);

        var deleted = await DeleteUserAclCore(repos, webSocketConnector, userId, secObjId);

        if (deleted)
        {
            resolver.Invalidate();
        }

        return deleted;
    }

    private static async Task<bool> DeleteUserAclCore(
        AppRepos repos, WebSocketConnector webSocketConnector, Guid userId, int secObjId)
    {
        var deleted = await repos.UserAcls.DeleteById(new UserAclKey(userId, secObjId));

        if (deleted)
        {
            await webSocketConnector.SendUserAclDeletedAsync(
                new WebsocketUserAclDeletedPayload { UserId = userId, SecObjId = secObjId });
        }

        return deleted;
    }

    #region Convenience wrappers - the frontend never handles sec_obj_id directly

    [Authorize]
    public Task<RoleAcl?> SetRoleAclForGlobal(
        AppRepos repos, IPermissionResolver resolver, WebSocketConnector webSocketConnector,
        int roleId, Permission[] permissions) =>
        SetRoleAcl(repos, resolver, webSocketConnector, roleId, SecureObjectIds.Global, permissions);

    /// <summary>
    /// Authorization runs BEFORE the workspace/channelType/channel lookup on every wrapper below -
    /// otherwise a NOT_FOUND vs FORBIDDEN response would let any authenticated user enumerate valid
    /// ids without RoleEditRole/UserEditUser.
    /// </summary>
    [Authorize]
    public async Task<RoleAcl?> SetRoleAclForWorkspace(
        AppRepos repos, IPermissionResolver resolver, WebSocketConnector webSocketConnector,
        int roleId, int workspaceId, Permission[] permissions)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.RoleEditRole);

        var secObjId = await ResolveWorkspaceSecObjId(repos, workspaceId);
        return await SetRoleAcl(repos, resolver, webSocketConnector, roleId, secObjId, permissions);
    }

    [Authorize]
    public async Task<RoleAcl?> SetRoleAclForChannelType(
        AppRepos repos, IPermissionResolver resolver, WebSocketConnector webSocketConnector,
        int roleId, int channelTypeId, Permission[] permissions)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.RoleEditRole);

        var secObjId = await ResolveChannelTypeSecObjId(repos, channelTypeId);
        return await SetRoleAcl(repos, resolver, webSocketConnector, roleId, secObjId, permissions);
    }

    [Authorize]
    public async Task<RoleAcl?> SetRoleAclForChannel(
        AppRepos repos, IPermissionResolver resolver, WebSocketConnector webSocketConnector,
        int roleId, int channelId, Permission[] permissions)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.RoleEditRole);

        var secObjId = await ResolveChannelSecObjId(repos, channelId);
        return await SetRoleAcl(repos, resolver, webSocketConnector, roleId, secObjId, permissions);
    }

    [Authorize]
    public Task<UserAcl?> SetUserAclForGlobal(
        AppRepos repos, IPermissionResolver resolver, WebSocketConnector webSocketConnector,
        Guid userId, Permission[] permissions) =>
        SetUserAcl(repos, resolver, webSocketConnector, userId, SecureObjectIds.Global, permissions);

    [Authorize]
    public async Task<UserAcl?> SetUserAclForWorkspace(
        AppRepos repos, IPermissionResolver resolver, WebSocketConnector webSocketConnector,
        Guid userId, int workspaceId, Permission[] permissions)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.UserEditUser);

        var secObjId = await ResolveWorkspaceSecObjId(repos, workspaceId);
        return await SetUserAcl(repos, resolver, webSocketConnector, userId, secObjId, permissions);
    }

    [Authorize]
    public async Task<UserAcl?> SetUserAclForChannelType(
        AppRepos repos, IPermissionResolver resolver, WebSocketConnector webSocketConnector,
        Guid userId, int channelTypeId, Permission[] permissions)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.UserEditUser);

        var secObjId = await ResolveChannelTypeSecObjId(repos, channelTypeId);
        return await SetUserAcl(repos, resolver, webSocketConnector, userId, secObjId, permissions);
    }

    [Authorize]
    public async Task<UserAcl?> SetUserAclForChannel(
        AppRepos repos, IPermissionResolver resolver, WebSocketConnector webSocketConnector,
        Guid userId, int channelId, Permission[] permissions)
    {
        var actorPermissions = await resolver.Global();
        actorPermissions.Require(Permission.UserEditUser);

        var secObjId = await ResolveChannelSecObjId(repos, channelId);
        return await SetUserAcl(repos, resolver, webSocketConnector, userId, secObjId, permissions);
    }

    #endregion

    private static async Task<int> ResolveWorkspaceSecObjId(AppRepos repos, int workspaceId)
    {
        var workspace = await repos.Workspaces.GetById(workspaceId);

        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        return workspace!.SecObjId;
    }

    private static async Task<int> ResolveChannelTypeSecObjId(AppRepos repos, int channelTypeId)
    {
        var channelType = await repos.ChannelTypes.GetById(channelTypeId);

        if (channelType == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        return channelType!.SecObjId;
    }

    private static async Task<int> ResolveChannelSecObjId(AppRepos repos, int channelId)
    {
        var channel = await repos.Channels.GetById(channelId);

        if (channel == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        return channel!.SecObjId;
    }
}
