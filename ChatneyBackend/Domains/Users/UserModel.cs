using ChatneyBackend.Infra;
using RepoDb.Attributes;
using ChatneyBackend.Utils;
using System.Linq.Expressions;

namespace ChatneyBackend.Domains.Users;

public readonly record struct UserRoleKey(Guid UserId, int RoleId);

/// <summary>Assignment of a role to a user. A user can hold several roles.</summary>
public class UserRole : IPgKey<UserRole, UserRoleKey>
{
    [Primary]
    [Map("user_id")]
    public required Guid UserId { get; set; }

    [Primary]
    [Map("role_id")]
    public required int RoleId { get; set; }

    public static Expression<Func<UserRole, bool>> MatchByKey(UserRoleKey key) =>
        role => role.UserId == key.UserId && role.RoleId == key.RoleId;

    public static UserRoleKey GetKey(UserRole record) => new(record.UserId, record.RoleId);
}

// TODO: move to another model/table
public class ChannelSettings
{
    public required string LastSeenMessage { get; set; }

    public bool Muted { get; set; }
}

/// <summary>A user account.</summary>
public class User : IPgKey<User, Guid>, IPgTimestamped
{
    [Primary]
    [Map("id")]
    public Guid Id { get; set; }

    /// <summary>Unique handle: 1-20 chars of [a-zA-Z0-9_].</summary>
    [Map("nickname")]
    public required string Nickname { get; set; }

    [Map("full_name")]
    public string? FullName { get; set; }

    /// <summary>Inactive users are hidden from user search.</summary>
    [Map("active")]
    public bool Active { get; set; }

    /// <summary>Email address has been verified.</summary>
    [Map("verified")]
    public bool Verified { get; set; }

    /// <summary>Banned users are hidden from user search.</summary>
    [Map("banned")]
    public bool Banned { get; set; }

    [Map("muted")]
    public bool Muted { get; set; }

    [Map("email")]
    public required string Email { get; set; }

    [Map("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Map("password")]
    [GraphQLIgnore]
    public required string Password { get; set; }

    [Map("created_at")]
    [GraphQLIgnore]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    [GraphQLIgnore]
    public DateTime UpdatedAt { get; set; }

    public static Expression<Func<User, bool>> MatchByKey(Guid key) => user => user.Id == key;

    public static Guid GetKey(User record) => record.Id;
}

/// <summary>Input for self-registration.</summary>
public class UserRegisterDto : IDto<User>
{
    /// <summary>1-20 chars of [a-zA-Z0-9_], trimmed. INVALID_NICKNAME if malformed, NICKNAME_TAKEN if in use.</summary>
    public required string Nickname { get; set; }

    public string? FullName { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public User ToModel()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Nickname = Nickname,
            FullName = string.IsNullOrWhiteSpace(FullName) ? null : FullName.Trim(),
            Email = Email,
            Password = Password,
            // TODO: set to TRUE if email confirmation required
            Active = true,
            Muted = false,
            Banned = false,
            Verified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}

/// <summary>Input for an admin creating a user.</summary>
public class CreateUserDto : IDto<User>
{
    /// <summary>1-20 chars of [a-zA-Z0-9_], trimmed. INVALID_NICKNAME if malformed, NICKNAME_TAKEN if in use.</summary>
    public required string Nickname { get; set; }

    public string? FullName { get; set; }

    public bool Active { get; set; }

    public bool Verified { get; set; }

    public bool Banned { get; set; }

    public bool Muted { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    /// <summary>Roles to assign. Null or empty assigns the configured default role.</summary>
    public List<int>? RoleIds { get; set; }

    public User ToModel()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Nickname = Nickname,
            FullName = string.IsNullOrWhiteSpace(FullName) ? null : FullName.Trim(),
            Email = Email,
            Password = Password,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Active = Active,
            Banned = Banned,
            Verified = Verified,
            Muted = Muted,
        };
    }
}

/// <summary>Input for an admin updating a user.</summary>
public class UpdateUserDto
{
    public Guid Id { get; set; }

    /// <summary>1-20 chars of [a-zA-Z0-9_], trimmed. INVALID_NICKNAME if malformed, NICKNAME_TAKEN if in use.</summary>
    public required string Nickname { get; set; }

    public string? FullName { get; set; }

    public bool Active { get; set; }

    public bool Verified { get; set; }

    public bool Banned { get; set; }

    public bool Muted { get; set; }

    public required string Email { get; set; }

    /// <summary>New password. Null or blank keeps the current one.</summary>
    public string? Password { get; set; }

    /// <summary>Full replacement set of the user's roles.</summary>
    public required List<int> RoleIds { get; set; }
}

/// <summary>Input for updating your own profile. Null fields are left unchanged.</summary>
public class UpdateMyProfileDto
{
    /// <summary>1-20 chars of [a-zA-Z0-9_], trimmed. INVALID_NICKNAME if malformed, NICKNAME_TAKEN if in use.</summary>
    public string? Nickname { get; set; }

    /// <summary>Empty string clears it.</summary>
    public string? FullName { get; set; }

    public string? Email { get; set; }

    /// <summary>Empty string clears it.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Required when changing the password. FORBIDDEN_ACTION if missing or wrong.</summary>
    public string? CurrentPassword { get; set; }

    /// <summary>New password. Requires currentPassword.</summary>
    public string? NewPassword { get; set; }
}

/// <summary>Result of a successful login.</summary>
public class UserLoginResponse
{
    public required string Id { get; set; }

    /// <summary>JWT to send as a Bearer token on subsequent requests.</summary>
    public required string Token { get; set; }
}

public class WebsocketUserRolePayload
{
    public Guid UserId { get; set; }
    public int RoleId { get; set; }

    public static WebsocketUserRolePayload FromUserRole(UserRole userRole) => new()
    {
        UserId = userRole.UserId,
        RoleId = userRole.RoleId,
    };
}

public class WebsocketUserRoleDeletedPayload
{
    public Guid UserId { get; set; }
    public int RoleId { get; set; }

    public static WebsocketUserRoleDeletedPayload FromKey(UserRoleKey key) => new()
    {
        UserId = key.UserId,
        RoleId = key.RoleId,
    };
}
