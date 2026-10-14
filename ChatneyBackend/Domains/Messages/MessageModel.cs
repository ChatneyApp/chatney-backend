using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Messages;

/// <summary>Aggregated reactions of one code on a message.</summary>
public class ReactionInMessage
{
    /// <summary>Reaction code (e.g. an emoji shortcode).</summary>
    [Map("code")]
    [MaxLength(255)]
    public required string Code { get; set; }

    /// <summary>Number of users who reacted with this code.</summary>
    [Map("count")]
    public required int Count { get; set; }
}

/// <summary>A chat message.</summary>
public class Message : IPgKey<Message, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("channel_id")]
    public required int ChannelId { get; set; }

    /// <summary>Author.</summary>
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
    /// <summary>Delivery state. Currently always "sent".</summary>
    [Map("status")]
    [MaxLength(50)]
    public required string Status { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>Thread root message id if this is a thread reply, otherwise null.</summary>
    [Map("parent_id")]
    public int? ParentId { get; set; }

    /// <summary>Number of thread replies (only meaningful on thread roots).</summary>
    [Map("children_count")]
    public required int ChildrenCount { get; set; }

    /// <summary>Id of the message this one quote-replies to. Its preview is in MessagesResult.refs.</summary>
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

    public static int GetKey(Message record) => record.Id;
}

/// <summary>Input for posting a message.</summary>
public class MessageDto
{
    public required int ChannelId { get; set; }

    [MaxLength(4096)]
    public required string Content { get; set; }

    /// <summary>Ids of attachments uploaded beforehand via attachments.upload.</summary>
    public int[]? AttachmentIds { get; set; }

    /// <summary>Thread root message id to post as a thread reply.</summary>
    public int? ParentId { get; set; }

    /// <summary>Message id to quote-reply to.</summary>
    public int? ReplyTo { get; set; }
}

/// <summary>Input for editing a message.</summary>
public class MessageUpdateDto
{
    public required int Id { get; set; }

    [MaxLength(4096)]
    public required string Content { get; set; }

    /// <summary>Full replacement set of attachments. Null removes all.</summary>
    public int[]? AttachmentIds { get; set; }
}


/// <summary>Public info of a message's author.</summary>
public class MessageUser
{
    public required Guid Id { get; set; }

    public required string Nickname { get; set; }

    public string? FullName { get; set; }

    public required string? AvatarUrl { get; set; }

    /// <summary>fullName if set, otherwise nickname.</summary>
    public string DisplayName => FullName ?? Nickname;
}

/// <summary>Short preview of a quote-replied message.</summary>
public class ReplyToMessage
{
    public required int Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Content { get; set; }
}

/// <summary>A list of messages plus previews of the messages they quote-reply to.</summary>
public class MessagesResult
{
    public required List<MessageWithUser> Messages { get; set; }
    /// <summary>Previews of messages referenced by replyTo in messages.</summary>
    public required List<ReplyToMessage> Refs { get; set; }
}

/// <summary>A message with its author, attachments, URL previews and reactions resolved.</summary>
public class MessageWithUser : Message
{
    public required MessageUser User { get; set; }
    /// <summary>Reaction codes the current user has added to this message.</summary>
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
                Nickname = user.Nickname,
                FullName = user.FullName,
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
