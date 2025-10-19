using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Domains.Users;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Messages;

public class Message : DatabaseItem, IHasUserId
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
    public List<ReactionInMessage> Reactions { get; set; }

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
    public string ChannelId { get; set; }

    [BsonElement("content")]
    [MaxLength(4096)]
    public string Content { get; set; }

    [BsonElement("attachments")]
    public List<string> Attachments { get; set; }

    [BsonElement("parentId")]
    [MaxLength(36)]
    public string? ParentId { get; set; }
}


public class MessageUser
{
    [BsonElement("_id")]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }
}

public class MessageWithUser : Message
{
    [BsonElement("user")]
    public MessageUser User { get; set; }

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
    public string MessageId { get; set; }
    public string ChannelId { get; set; }
}

public class MessageAttachment : DatabaseItem
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("path")]
    [MaxLength(36)]
    public string Path { get; set; }

    [BsonElement("fileName")]
    [MaxLength(36)]
    public string FileName { get; set; }

    [BsonElement("mimeType")]
    [MaxLength(36)]
    public string MimeType { get; set; }

    [BsonElement("createdBy")]
    [MaxLength(36)]
    public string CreatedBy { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
