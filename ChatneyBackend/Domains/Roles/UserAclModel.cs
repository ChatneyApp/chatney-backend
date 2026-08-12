using System.Linq.Expressions;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Roles;

public readonly record struct UserAclKey(Guid UserId, int SecObjId);

public class UserAcl : IPgKey<UserAcl, UserAclKey>
{
    [Primary]
    [Map("user_id")]
    public required Guid UserId { get; set; }

    [Primary]
    [Map("sec_obj_id")]
    public required int SecObjId { get; set; }

    [Map("permissions")]
    public Permission[] Permissions { get; set; } = [];

    public static Expression<Func<UserAcl, bool>> MatchByKey(UserAclKey key) =>
        acl => acl.UserId == key.UserId && acl.SecObjId == key.SecObjId;

    public static UserAclKey GetKey(UserAcl record) => new(record.UserId, record.SecObjId);
}

public class WebsocketUserAclPayload
{
    public Guid UserId { get; set; }
    public int SecObjId { get; set; }
    public Permission[] Permissions { get; set; } = [];

    public static WebsocketUserAclPayload FromUserAcl(UserAcl acl) => new()
    {
        UserId = acl.UserId,
        SecObjId = acl.SecObjId,
        Permissions = acl.Permissions,
    };
}

public class WebsocketUserAclDeletedPayload
{
    public Guid UserId { get; set; }
    public int SecObjId { get; set; }
}
