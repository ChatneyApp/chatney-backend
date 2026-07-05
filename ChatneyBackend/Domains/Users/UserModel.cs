using ChatneyBackend.Infra;
using RepoDb.Attributes;
using ChatneyBackend.Utils;
using System.Linq.Expressions;

namespace ChatneyBackend.Domains.Users;

public readonly record struct UserRoleKey(Guid UserId, int? ChannelId, int? ChannelTypeId, int? WorkspaceId);

public class UserRole : IPgKey<UserRole, UserRoleKey>
{
    [Primary]
    [Map("user_id")]
    public Guid UserId { get; set; }

    [Primary]
    [Map("channel_id")]
    public int? ChannelId { get; set; }

    [Primary]
    [Map("channel_type_id")]
    public int? ChannelTypeId { get; set; }

    [Primary]
    [Map("workspace_id")]
    public int? WorkspaceId { get; set; }

    [Map("role_id")]
    public required int RoleId { get; set; }

    [Map("allowlist")]
    public required string[] Allowlist { get; set; }

    [Map("denylist")]
    public required string[] Denylist { get; set; }

    public static Expression<Func<UserRole, bool>> MatchByKey(UserRoleKey key) =>
        role => role.UserId == key.UserId &&
                role.ChannelId == key.ChannelId &&
                role.ChannelTypeId == key.ChannelTypeId &&
                role.WorkspaceId == key.WorkspaceId;
}

// TODO: move to another model/table
public class ChannelSettings
{
    public required string LastSeenMessage { get; set; }

    public bool Muted { get; set; }
}

public class User : IPgKey<User, Guid>, IPgTimestamped, IType
{
    [Primary]
    [Identity]
    [Map("id")]
    public Guid Id { get; set; }

    [Map("name")]
    public required string Name { get; set; }

    [Map("active")]
    public bool Active { get; set; }

    [Map("verified")]
    public bool Verified { get; set; }

    [Map("banned")]
    public bool Banned { get; set; }

    [Map("muted")]
    public bool Muted { get; set; }

    [Map("email")]
    public required string Email { get; set; }

    [Map("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Map("role_id")]
    [GraphQLIgnore]
    public required int RoleId { get; set; }

    [Map("password")]
    [GraphQLIgnore]
    public required string Password { get; set; }

    [Map("created_at")]
    [GraphQLIgnore]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    [GraphQLIgnore]
    public DateTime UpdatedAt { get; set; }

    [GraphQLIgnore]
    public TypeKind Kind { get; }

    public static Expression<Func<User, bool>> MatchByKey(Guid key) => user => user.Id == key;
}

/// <summary>
/// User registers themselves
/// </summary>
public class UserRegisterDto : IDto<User>
{
    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public User ToModel()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Email = Email,
            Password = Password,
            Active = false,
            Muted = false,
            Banned = false,
            Verified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RoleId = 0,
        };
    }
}

/// <summary>
/// Admin creates a user
/// </summary>
public class CreateUserDto : IDto<User>
{
    public required string Name { get; set; }

    public bool Active { get; set; }

    public bool Verified { get; set; }

    public bool Banned { get; set; }

    public bool Muted { get; set; }

    public required string Email { get; set; }

    public required int RoleId { get; set; }

    public required string Password { get; set; }

    public User ToModel()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Email = Email,
            Password = Password,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RoleId = RoleId,
            Active = Active,
            Banned = Banned,
            Verified = Verified,
            Muted = Muted,
        };
    }
}

public class UserLoginResponse
{
    public required string Id { get; set; }

    public required string Token { get; set; }
}

public class WebsocketUserRolePayload
{
    public Guid UserId { get; set; }
    public int? ChannelId { get; set; }
    public int? ChannelTypeId { get; set; }
    public int? WorkspaceId { get; set; }
    public int RoleId { get; set; }
    public string[] Allowlist { get; set; } = [];
    public string[] Denylist { get; set; } = [];

    public static WebsocketUserRolePayload FromUserRole(UserRole userRole) => new()
    {
        UserId = userRole.UserId,
        ChannelId = userRole.ChannelId,
        ChannelTypeId = userRole.ChannelTypeId,
        WorkspaceId = userRole.WorkspaceId,
        RoleId = userRole.RoleId,
        Allowlist = userRole.Allowlist,
        Denylist = userRole.Denylist,
    };
}

public class WebsocketUserRoleDeletedPayload
{
    public Guid UserId { get; set; }
    public int? ChannelId { get; set; }
    public int? ChannelTypeId { get; set; }
    public int? WorkspaceId { get; set; }

    public static WebsocketUserRoleDeletedPayload FromKey(UserRoleKey key) => new()
    {
        UserId = key.UserId,
        ChannelId = key.ChannelId,
        ChannelTypeId = key.ChannelTypeId,
        WorkspaceId = key.WorkspaceId,
    };
}
