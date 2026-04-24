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

public class Message : IPgKey<Message, int>, IPgTimestamped
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
    public int[] AttachmentIds { get; set; } = [];

    [Map("url_preview_ids")]
    [GraphQLIgnore]
    [JsonIgnore]
    public int[] UrlPreviewIds { get; set; } = [];

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

    [Map("reply_to")]
    public int? ReplyTo { get; set; }

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
            ChildrenCount = 0,
            ReplyTo = message.ReplyTo
        };
    }

    public static Expression<Func<Message, bool>> MatchByKey(int key) => message => message.Id == key;
}

public class MessageDto
{
    public required int ChannelId { get; set; }

    [MaxLength(4096)]
    public required string Content { get; set; }

    public int[]? AttachmentIds { get; set; }

    public int? ParentId { get; set; }

    public int? ReplyTo { get; set; }
}

public class MessageUpdateDto
{
    public required int Id { get; set; }

    [MaxLength(4096)]
    public required string Content { get; set; }

    public int[]? AttachmentIds { get; set; }
}


public class MessageUser
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required string? AvatarUrl { get; set; }
}

public class ReplyToMessage
{
    public required int Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Content { get; set; }
}

public class MessagesResult
{
    public required List<MessageWithUser> Messages { get; set; }
    public required List<ReplyToMessage> Refs { get; set; }
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
            ReplyTo = message.ReplyTo,
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

public class NewMessagePayload
{
    public required MessageWithUser Message { get; set; }
    public ReplyToMessage? ReplyTo { get; set; }
}

public class DeletedMessage
{
    public required int MessageId { get; set; }
    public required int ChannelId { get; set; }
}

public class EditedMessagePayload
{
    public required MessageWithUser Message { get; set; }
}

public class MessageChildrenCountUpdated
{
    public required int MessageId { get; set; }
    public required int ChildrenCount { get; set; }
}
