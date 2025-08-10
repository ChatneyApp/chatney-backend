using System.ComponentModel.DataAnnotations;
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
    [BsonElement("workspaceId")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string WorkspaceId { get; set; }

    [BsonElement("roleId")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string RoleId { get; set; }
}

public class RoleChannel
{
    [BsonElement("channelId")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string ChannelId { get; set; }

    [BsonElement("roleId")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string RoleId { get; set; }
}

public class RoleChannelType
{
    [BsonElement("channelTypeId")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string ChannelTypeId { get; set; }

    [BsonElement("roleId")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string RoleId { get; set; }
}

public class Role
{
    [BsonElement("global")]
    public required string Global { get; set; }

    [BsonElement("workspace")]
    public required List<RoleWorkspace> Workspace { get; set; }

    [BsonElement("channel")]
    public required List<RoleChannel> Channel { get; set; }

    [BsonElement("channel_types")]
    public required List<RoleChannelType> ChannelTypes { get; set; }
}

public class ChannelSettings
{
    [BsonElement("channelId")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string ChannelId { get; set; }

    [BsonElement("lastSeenMessage")]
    public required string LastSeenMessage { get; set; }

    [BsonElement("muted")]
    public required bool Muted { get; set; }
}

public class User
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public required UserStatus Status { get; set; }

    [BsonElement("email")]
    public required string Email { get; set; }

    [BsonElement("roles")]
    public required Role Roles { get; set; }

    [BsonElement("channelsSettings")]
    public required List<ChannelSettings> ChannelsSettings { get; set; }

    [BsonElement("workspaces")]
    public required List<string> Workspaces { get; set; }

    [BsonElement("password")]
    public required string Password { get; set; }
}
