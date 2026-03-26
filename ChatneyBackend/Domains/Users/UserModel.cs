using ChatneyBackend.Infra;
using RepoDb.Attributes;
using ChatneyBackend.Utils;
using System.Linq.Expressions;

namespace ChatneyBackend.Domains.Users;

public readonly record struct UserRoleKey(Guid UserId, string Type, string ItemId);

public class UserRole : IPgKey<UserRole, UserRoleKey>
{
    [Primary]
    [Map("user_id")]
    public required Guid UserId { get; set; }

    /// <summary>
    /// "workspace" | "channel" | "channel_type"
    /// </summary>
    [Primary]
    [Map("type")]
    public required string Type { get; set; }

    [Primary]
    [Map("item_id")]
    public required string ItemId { get; set; }

    [Map("role_id")]
    public required int RoleId { get; set; }

    public static Expression<Func<UserRole, bool>> MatchByKey(UserRoleKey key) =>
        role => role.UserId == key.UserId && role.Type == key.Type && role.ItemId == key.ItemId;
}

public class ChannelSettings
{
    public string LastSeenMessage { get; set; }

    public bool Muted { get; set; }
}

public class User : IPgKey<User, Guid>, IPgTimestamped, IType
{
    [Primary]
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

    // [Map("channels_settings")]
    // [GraphQLIgnore]
    // public Dictionary<string, ChannelSettings>? ChannelsSettings { get; set; }

    [Map("workspaces")]
    public required string[] Workspaces { get; set; }

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
public class UserRegisterDto : IDTO<User>
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
            Workspaces = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RoleId = 0,
            // ChannelsSettings = new Dictionary<string, ChannelSettings>()
        };
    }
}

/// <summary>
/// Admin creates a user
/// </summary>
public class CreateUserDto : IDTO<User>
{
    public required string Name { get; set; }

    public bool Active { get; set; }

    public bool Verified { get; set; }

    public bool Banned { get; set; }

    public bool Muted { get; set; }

    public required string Email { get; set; }

    public required int RoleId { get; set; }

    public Dictionary<string, ChannelSettings>? ChannelsSettings { get; set; }

    public required string[] Workspaces { get; set; }

    public required string Password { get; set; }

    public User ToModel()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Email = Email,
            Password = Password,
            Workspaces = Workspaces,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RoleId = RoleId,
            // ChannelsSettings = ChannelsSettings,
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

public interface IHasUserId
{
    Guid UserId { get; }
}
