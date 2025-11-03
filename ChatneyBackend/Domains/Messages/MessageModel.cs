using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Domains.Users;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Messages;

public class ReactionInMessage
{
    [BsonElement("code")]
    [MaxLength(255)]
    public required string Code { get; set; }

    [BsonElement("count")]
    public required int Count { get; set; }
}

public class Message : DatabaseItem, IHasUserId
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

    [BsonElement("urlPreviews")]
    public List<string> UrlPreviewIds { get; set; } = new();

    [BsonElement("status")]
    [MaxLength(50)]
    public required string Status { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("reactions")]
    public required List<ReactionInMessage> Reactions { get; set; } = new List<ReactionInMessage>();

    [BsonElement("parentId")]
    [MaxLength(36)]
    public string? ParentId { get; set; }

    public static Message FromDTO(MessageDTO message, string userId)
    {
        return new Message()
        {
            Id = Guid.NewGuid().ToString(),
            ChannelId = message.ChannelId,
            UserId = userId,
            Content = message.Content,
            Attachments = message.Attachments,
            Status = "sent", // TODO: Define status constants
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Reactions = new List<ReactionInMessage>(),
            ParentId = message.ParentId
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


public class MessageUser
{
    [BsonElement("_id")]
    public required string Id { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("avatarUrl")]
    public required string? AvatarUrl { get; set; }
}

public class MessageWithUser : Message
{
    [BsonElement("user")]
    public required MessageUser User { get; set; }
    [BsonElement("myReactions")]
    public required string[] MyReactions { get; set; }

    public static MessageWithUser Create(Message message, User user)
    {
        return new MessageWithUser()
        {
            Id = message.Id,
            ChannelId = message.ChannelId,
            UserId = user.Id,
            Content = message.Content,
            Attachments = message.Attachments,
            Status = message.Status,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            Reactions = message.Reactions,
            ParentId = message.ParentId,
            MyReactions = [],
            User = new MessageUser()
            {
                Id = user.Id,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
            }
        };

    }
}

public class DeletedMessage
{
    public required string MessageId { get; set; }
    public required string ChannelId { get; set; }
}

public class MessageAttachment : DatabaseItem
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("path")]
    [MaxLength(36)]
    public required string Path { get; set; }

    [BsonElement("fileName")]
    [MaxLength(36)]
    public required string FileName { get; set; }

    [BsonElement("mimeType")]
    [MaxLength(36)]
    public required string MimeType { get; set; }

    [BsonElement("createdBy")]
    [MaxLength(36)]
    public required string CreatedBy { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
