using ChatneyBackend.Infra;
using RepoDb.Attributes;
using ChatneyBackend.Utils;
using System.Linq.Expressions;

namespace ChatneyBackend.Domains.Users;

public readonly record struct UserRoleKey(Guid UserId, int RoleId);

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

public class User : IPgKey<User, Guid>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public Guid Id { get; set; }

    [Map("nickname")]
    public required string Nickname { get; set; }

    [Map("full_name")]
    public string? FullName { get; set; }

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

/// <summary>
/// User registers themselves
/// </summary>
public class UserRegisterDto : IDto<User>
{
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
            Active = false,
            Muted = false,
            Banned = false,
            Verified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}

/// <summary>
/// Admin creates a user
/// </summary>
public class CreateUserDto : IDto<User>
{
    public required string Nickname { get; set; }

    public string? FullName { get; set; }

    public bool Active { get; set; }

    public bool Verified { get; set; }

    public bool Banned { get; set; }

    public bool Muted { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

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

public class UpdateUserDto
{
    public Guid Id { get; set; }

    public required string Nickname { get; set; }

    public string? FullName { get; set; }

    public bool Active { get; set; }

    public bool Verified { get; set; }

    public bool Banned { get; set; }

    public bool Muted { get; set; }

    public required string Email { get; set; }

    public string? Password { get; set; }

    public required List<int> RoleIds { get; set; }
}

public class UpdateMyProfileDto
{
    public string? Nickname { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? AvatarUrl { get; set; }

    public string? CurrentPassword { get; set; }

    public string? NewPassword { get; set; }
}

public class UserLoginResponse
{
    public required string Id { get; set; }

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
