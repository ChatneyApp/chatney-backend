using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Domains.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessage : DatabaseItem, IHasUserId
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("channelId")]
    [MaxLength(36)]
    public required string ChannelId { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    [MaxLength(36)]
    public required Guid UserId { get; set; }

    [BsonElement("content")]
    [MaxLength(4096)]
    public required string Content { get; set; }

    [BsonElement("attachments")]
    public List<string> Attachments { get; set; } = new List<string>();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("parentId")]
    [MaxLength(36)]
    public string? ParentId { get; set; }

    public static DraftMessage FromDto(MessageDto message, Guid userId) =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            ChannelId = message.ChannelId,
            UserId = userId,
            Content = message.Content,
            Attachments = message.Attachments,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ParentId = message.ParentId,
        };
}

public class MessageDto
{
    [MaxLength(36)]
    public required string ChannelId { get; set; }

    [MaxLength(4096)]
    public required string Content { get; set; }

    public List<string>? Attachments { get; set; }

    [MaxLength(36)]
    public string? ParentId { get; set; }
}
