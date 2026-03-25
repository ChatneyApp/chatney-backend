using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Users;

public class UserRole
{
    public int Global { get; set; }

    [GraphQLIgnore]
    public Dictionary<string, int>? Workspace { get; set; }

    [GraphQLIgnore]
    public Dictionary<string, int>? Channel { get; set; }

    [GraphQLIgnore]
    public Dictionary<string, int>? ChannelTypes { get; set; }
}

public class ChannelSettings
{
    public required string LastSeenMessage { get; set; }

    public bool Muted { get; set; }
}

public class UserResponse
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public bool Active { get; set; }
    public bool Verified { get; set; }
    public bool Banned { get; set; }
    public bool Muted { get; set; }

    public required string Email { get; set; }

    public required UserRole Roles { get; set; }

    [GraphQLIgnore]
    public Dictionary<string, ChannelSettings>? ChannelsSettings { get; set; }

    public required string[] Workspaces { get; set; }
}

public class User : IPgDatabaseItem<Guid>
{
    [Primary]
    [Map("id")]
    public Guid Id { get; set; }

    [Map("name")]
    [MaxLength(255)]
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
    [MaxLength(255)]
    public required string Email { get; set; }

    [Map("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Map("global_role_id")]
    public int GlobalRoleId { get; set; }

    /// <summary>JSON-serialized Dictionary&lt;string, int&gt; stored as jsonb.</summary>
    [Map("workspace_roles")]
    [GraphQLIgnore]
    public string WorkspaceRoles { get; set; } = "{}";

    /// <summary>JSON-serialized Dictionary&lt;string, int&gt; stored as jsonb.</summary>
    [Map("channel_roles")]
    [GraphQLIgnore]
    public string ChannelRoles { get; set; } = "{}";

    /// <summary>JSON-serialized Dictionary&lt;string, int&gt; stored as jsonb.</summary>
    [Map("channel_type_roles")]
    [GraphQLIgnore]
    public string ChannelTypeRoles { get; set; } = "{}";

    /// <summary>JSON-serialized Dictionary&lt;string, ChannelSettings&gt; stored as jsonb.</summary>
    [Map("channels_settings")]
    [GraphQLIgnore]
    public string ChannelsSettingsJson { get; set; } = "{}";

    [Map("workspaces")]
    public string[] Workspaces { get; set; } = [];

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
    public UserRole Roles => new()
    {
        Global = GlobalRoleId,
        Workspace = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(WorkspaceRoles),
        Channel = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(ChannelRoles),
        ChannelTypes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(ChannelTypeRoles),
    };

    [GraphQLIgnore]
    public Dictionary<string, ChannelSettings>? ChannelsSettings =>
        System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, ChannelSettings>>(ChannelsSettingsJson);
}

/// <summary>
/// User registers themselves
/// </summary>
public class UserRegisterDTO
{
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(255)]
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
        };
    }
}

/// <summary>
/// Admin creates a user
/// </summary>
public class CreateUserDTO
{
    [MaxLength(255)]
    public required string Name { get; set; }

    public bool Active { get; set; }

    public bool Verified { get; set; }

    public bool Banned { get; set; }

    public bool Muted { get; set; }

    [MaxLength(255)]
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
            Workspaces = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Active = Active,
            Banned = Banned,
            Verified = Verified,
            Muted = Muted,
        };
    }
}

public class UserLoginResponse
{
    public Guid Id { get; set; }

    public required string Token { get; set; }
}

public interface IHasUserId
{
    string UserId { get; }
}
