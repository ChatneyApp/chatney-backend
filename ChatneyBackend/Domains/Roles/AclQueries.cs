using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Roles;

/// <summary>Every role_acls / user_acls row attached directly to one secure object (not resolved
/// through the hierarchy - just what's stored on this object). The minimum admin-UI affordance for
/// seeing WHY a permission resolved as it did.</summary>
public record ObjectAcls
{
    public required List<RoleAcl> RoleAcls { get; init; }
    public required List<UserAcl> UserAcls { get; init; }
}

public class AclQueries
{
    [Authorize]
    public async Task<List<RoleAcl>> RoleAcls(AppRepos repos, IPermissionResolver resolver, int roleId)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.RoleEditRole);

        return await repos.RoleAcls.GetList(acl => acl.RoleId == roleId);
    }

    [Authorize]
    public async Task<List<UserAcl>> UserAcls(AppRepos repos, IPermissionResolver resolver, Guid userId)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.UserEditUser);

        return await repos.UserAcls.GetList(acl => acl.UserId == userId);
    }

    /// <summary>
    /// Requires RoleEditRole for the role_acls portion, but the user_acls portion is only included if
    /// the actor ALSO has UserEditUser - otherwise a role-admin who is deliberately not a user-admin
    /// could enumerate every user's per-object grants through this endpoint (see AclQueries tests).
    /// </summary>
    [Authorize]
    public async Task<ObjectAcls> ObjectAcls(AppRepos repos, IPermissionResolver resolver, int secObjId)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.RoleEditRole);

        var roleAcls = await repos.RoleAcls.GetList(acl => acl.SecObjId == secObjId);
        var userAcls = permissions.Can(Permission.UserEditUser)
            ? await repos.UserAcls.GetList(acl => acl.SecObjId == secObjId)
            : [];

        return new ObjectAcls { RoleAcls = roleAcls, UserAcls = userAcls };
    }
}
