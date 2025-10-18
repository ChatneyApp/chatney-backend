using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChatneyBackend.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Users;

public class RoleWorkspace
{
    [BsonElement("roleId")]
    [MaxLength(36)]
    public string RoleId { get; set; }
}

public class RoleChannel
{
    [BsonElement("roleId")]
    [MaxLength(36)]
    public string RoleId { get; set; }
}

public class RoleChannelType
{
    [BsonElement("roleId")]
    [MaxLength(36)]
    public string RoleId { get; set; }
}

public class UserRole
{
    [BsonElement("global")]
    public string Global { get; set; }

    [BsonElement("workspaces")]
    [NotMapped]
    public Dictionary<string, RoleWorkspace> Workspace { get; set; }

    [BsonElement("channels")]
    [NotMapped]
    public Dictionary<string, RoleChannel> Channel { get; set; }

    [BsonElement("channelTypes")]
    [NotMapped]
    public Dictionary<string, RoleChannelType> ChannelTypes { get; set; }
}

public class ChannelSettings
{
    [BsonElement("lastSeenMessage")]
    public string LastSeenMessage { get; set; }

    [BsonElement("muted")]
    public bool Muted { get; set; }
}

public class UserResponse
{
    public string Id { get; set; }

    public string Name { get; set; }

    public bool Active { get; set; }
    public bool Verified { get; set; }
    public bool Banned { get; set; }
    public bool Muted { get; set; }

    public string Email { get; set; }

    public UserRole Roles { get; set; }

    [NotMapped] // hot chocolate attribute
    public Dictionary<string, ChannelSettings>? ChannelsSettings { get; set; }

    public List<string> Workspaces { get; set; }
}

public class User : DatabaseItem, IType
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("active")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool Active { get; set; }

    [BsonElement("verified")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool Verified { get; set; }

    [BsonElement("banned")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool Banned { get; set; }

    [BsonElement("muted")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool Muted { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("roles")]
    public UserRole Roles { get; set; }

    [BsonElement("channelsSettings")]
    [NotMapped]
    public Dictionary<string, ChannelSettings>? ChannelsSettings { get; set; }

    [BsonElement("workspaces")]
    public List<string> Workspaces { get; set; }

    [BsonElement("password")]
    [GraphQLIgnore]
    public string Password { get; set; }

    [BsonElement("createdAt")]
    [GraphQLIgnore]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    [GraphQLIgnore]
    public DateTime UpdatedAt { get; set; }

    [GraphQLIgnore]
    public TypeKind Kind { get; }
}

/// <summary>
/// User registers themselves
/// </summary>
public class UserRegisterDTO : IDTO<User>
{
    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("password")]
    public string Password { get; set; }

    public User ToModel()
    {
        return new User
        {
            Id = Guid.NewGuid().ToString(),
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
            Roles = new UserRole()
            {
                Channel = new Dictionary<string, RoleChannel>(),
                ChannelTypes = new Dictionary<string, RoleChannelType>(),
                Global = string.Empty,
                Workspace = new  Dictionary<string, RoleWorkspace>(),
            },
            ChannelsSettings = new Dictionary<string, ChannelSettings>()
        };
    }
}

/// <summary>
/// Admin creates a user
/// </summary>
public class CreateUserDTO : IDTO<User>
{
    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("active")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool Active { get; set; }

    [BsonElement("verified")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool Verified { get; set; }

    [BsonElement("banned")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool Banned { get; set; }

    [BsonElement("muted")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool Muted { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("roles")]
    public UserRole Roles { get; set; }

    [BsonElement("channelsSettings")]
    [NotMapped]
    public Dictionary<string, ChannelSettings>? ChannelsSettings { get; set; }

    [BsonElement("workspaces")]
    public List<string> Workspaces { get; set; }

    [BsonElement("password")]
    public string Password { get; set; }

    public User ToModel()
    {
        return new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = Name,
            Email = Email,
            Password = Password,
            Workspaces = Workspaces,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = Roles,
            ChannelsSettings = ChannelsSettings,
            Active = Active,
            Banned = Banned,
            Verified = Verified,
            Muted = Muted,
        };
    }
}

public class UserLoginResponse
{
    public string Id { get; set; }

    public string Token { get; set; }
}

public interface IHasUserId
{
    string UserId { get; }
}
