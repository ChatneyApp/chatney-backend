using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Domains.Users;
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
    [MaxLength(36)]
    public required string UserId { get; set; }

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

    public static DraftMessage FromDTO(MessageDTO message, string userId)
    {
        return new DraftMessage()
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
}

public class MessageDTO
{
    [BsonElement("channelId")]
    [MaxLength(36)]
    public required string ChannelId { get; set; }

    [BsonElement("content")]
    [MaxLength(4096)]
    public required string Content { get; set; }

    [BsonElement("attachments")]
    public List<string>? Attachments { get; set; }

    [BsonElement("parentId")]
    [MaxLength(36)]
    public string? ParentId { get; set; }
}
