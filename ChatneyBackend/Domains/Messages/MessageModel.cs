using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Messages;

public class Reaction
{
    [BsonElement("userId")]
    [MaxLength(36)]
    public string UserId { get; set; }

    [BsonElement("reaction")]
    [MaxLength(255)]
    public string ReactionText { get; set; }
}

public class Message
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("channelId")]
    [MaxLength(36)]
    public string ChannelId { get; set; }

    [BsonElement("userId")]
    [MaxLength(36)]
    public string UserId { get; set; }

    [BsonElement("content")]
    [MaxLength(4096)]
    public string Content { get; set; }

    [BsonElement("attachments")]
    public List<string> Attachments { get; set; }

    [BsonElement("status")]
    [MaxLength(50)]
    public string Status { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("reactions")]
    public List<Reaction> Reactions { get; set; }

    [BsonElement("parentId")]
    [MaxLength(36)]
    public string? ParentId { get; set; }

    public static Message FromDTO(MessageDTO message)
    {
        return new Message()
        {
            Id = Guid.NewGuid().ToString(),
            ChannelId = message.ChannelId,
            UserId = message.UserId,
            Content = message.Content,
            Attachments = message.Attachments,
            Status = "sent", // TODO: Define status constants
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Reactions = new List<Reaction>(),
            ParentId = message.ParentId
        };
    }
}

public class MessageDTO
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("channelId")]
    [MaxLength(36)]
    public string ChannelId { get; set; }

    [BsonElement("userId")]
    [MaxLength(36)]
    public string UserId { get; set; }

    [BsonElement("content")]
    [MaxLength(4096)]
    public string Content { get; set; }

    [BsonElement("attachments")]
    public List<string> Attachments { get; set; }

    [BsonElement("parentId")]
    [MaxLength(36)]
    public string? ParentId { get; set; }
}
