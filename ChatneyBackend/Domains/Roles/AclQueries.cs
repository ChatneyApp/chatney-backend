using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Roles;

/// <summary>
/// The ACLs attached directly to one secure object. They are not resolved through the
/// hierarchy, so use this to see why a permission resolved the way it did.
/// </summary>
public record ObjectAcls
{
    public required List<RoleAcl> RoleAcls { get; init; }
    public required List<UserAcl> UserAcls { get; init; }
}

public class AclQueries
{
    /// <summary>All ACLs of one role across every secure object. Requires RoleEditRole.</summary>
    /// <param name="roleId">Role to list ACLs for.</param>
    [Authorize]
    public async Task<List<RoleAcl>> RoleAcls(AppRepos repos, IPermissionResolver resolver, int roleId)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.RoleEditRole);

        return await repos.RoleAcls.GetList(acl => acl.RoleId == roleId);
    }

    /// <summary>All direct ACLs of one user across every secure object. Requires UserEditUser.</summary>
    /// <param name="userId">User to list ACLs for.</param>
    [Authorize]
    public async Task<List<UserAcl>> UserAcls(AppRepos repos, IPermissionResolver resolver, Guid userId)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.UserEditUser);

        return await repos.UserAcls.GetList(acl => acl.UserId == userId);
    }

    // The user_acls portion is gated on UserEditUser too - otherwise a role-admin who is deliberately
    // not a user-admin could enumerate every user's per-object grants through this endpoint (see
    // AclQueries tests).
    /// <summary>
    /// All role and user ACLs attached directly to one secure object. Requires RoleEditRole.
    /// userAcls is always empty unless the caller also has UserEditUser.
    /// </summary>
    /// <param name="secObjId">Secure object id (from a workspace, channel type or channel).</param>
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
