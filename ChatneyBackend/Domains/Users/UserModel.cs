using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Users;

public enum UserStatus
{
    [BsonRepresentation(BsonType.String)]
    Active,
    Inactive,
    Banned,
    Muted
}

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

    [BsonElement("workspace")]
    [NotMapped]
    public Dictionary<string, RoleWorkspace> Workspace { get; set; }

    [BsonElement("channel")]
    [NotMapped]
    public Dictionary<string, RoleChannel> Channel { get; set; }

    [BsonElement("channel_types")]
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

public class User
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public UserStatus Status { get; set; }

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
}
