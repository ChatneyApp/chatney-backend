using System.Linq.Expressions;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Roles;

public readonly record struct RoleAclKey(int RoleId, int SecObjId);

public class RoleAcl : IPgKey<RoleAcl, RoleAclKey>
{
    [Primary]
    [Map("role_id")]
    public required int RoleId { get; set; }

    [Primary]
    [Map("sec_obj_id")]
    public required int SecObjId { get; set; }

    [Map("permissions")]
    public Permission[] Permissions { get; set; } = [];

    public static Expression<Func<RoleAcl, bool>> MatchByKey(RoleAclKey key) =>
        acl => acl.RoleId == key.RoleId && acl.SecObjId == key.SecObjId;

    public static RoleAclKey GetKey(RoleAcl record) => new(record.RoleId, record.SecObjId);
}

public class WebsocketRoleAclPayload
{
    public int RoleId { get; set; }
    public int SecObjId { get; set; }
    public Permission[] Permissions { get; set; } = [];

    public static WebsocketRoleAclPayload FromRoleAcl(RoleAcl acl) => new()
    {
        RoleId = acl.RoleId,
        SecObjId = acl.SecObjId,
        Permissions = acl.Permissions,
    };
}

public class WebsocketRoleAclDeletedPayload
{
    public int RoleId { get; set; }
    public int SecObjId { get; set; }
}
