using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Messages;

public class ReactionInMessage
{
    [Map("code")]
    [MaxLength(255)]
    public required string Code { get; set; }

    [Map("count")]
    public required int Count { get; set; }
}

public class Message : IPgKey<Message, int>, IPgTimestamped, IHasUserId
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("channel_id")]
    public required int ChannelId { get; set; }

    [Map("user_id")]
    public required Guid UserId { get; set; }

    [Map("content")]
    [MaxLength(4096)]
    public required string Content { get; set; }

    [Map("attachment_ids")]
    [GraphQLIgnore]
    [JsonIgnore]
    public string[] AttachmentIds { get; set; } = [];

    [Map("url_preview_ids")]
    [GraphQLIgnore]
    [JsonIgnore]
    public string[] UrlPreviewIds { get; set; } = [];

    // Used for soft delete, pending etc
    [Map("status")]
    [MaxLength(50)]
    public required string Status { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Map("parent_id")]
    public int? ParentId { get; set; }

    [Map("children_count")]
    public required int ChildrenCount { get; set; }

    public static Message FromDto(MessageDto message, Guid userId)
    {
        return new Message()
        {
            ChannelId = message.ChannelId,
            UserId = userId,
            Content = message.Content,
            AttachmentIds = message.AttachmentIds ?? [],
            Status = "sent", // TODO: Define status constants
            UrlPreviewIds = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ParentId = message.ParentId,
            ChildrenCount = 0
        };
    }

    public static Expression<Func<Message, bool>> MatchByKey(int key) => message => message.Id == key;
}

public class MessageDto
{
    public required int ChannelId { get; set; }

    [MaxLength(4096)]
    public required string Content { get; set; }

    public string[]? AttachmentIds { get; set; }

    public int? ParentId { get; set; }
}


public class MessageUser
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required string? AvatarUrl { get; set; }
}

public class MessageWithUser : Message
{
    public required MessageUser User { get; set; }
    public required string[] MyReactions { get; set; }
    public required List<UrlPreview> UrlPreviews { get; set; }
    public required List<Attachment> Attachments { get; set; }
    public required List<ReactionInMessage> Reactions { get; set; }

    public static MessageWithUser Create(Message message, User user, List<UrlPreview> urlPreviews, List<Attachment> attachments)
    {
        return new MessageWithUser()
        {
            Id = message.Id,
            ChannelId = message.ChannelId,
            UserId = user.Id,
            Content = message.Content,
            AttachmentIds = message.AttachmentIds,
            Attachments = attachments,
            Status = message.Status,
            UrlPreviewIds = message.UrlPreviewIds,
            UrlPreviews = urlPreviews,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            Reactions = [],
            ParentId = message.ParentId,
            ChildrenCount = message.ChildrenCount,
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
    public required int MessageId { get; set; }
    public required int ChannelId { get; set; }
}

public class MessageChildrenCountUpdated
{
    public required int MessageId { get; set; }
    public required int ChildrenCount { get; set; }
}
