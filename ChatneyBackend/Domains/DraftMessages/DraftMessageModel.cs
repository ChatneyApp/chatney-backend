using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.DraftMessages;

/// <summary>An unsent message the user is composing. At most one per user, channel and thread (parentId).</summary>
public class DraftMessage : IPgKey<DraftMessage, int>, IPgTimestamped
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

    /// <summary>Attachments already uploaded for this draft.</summary>
    [Map("attachment_ids")]
    public int[] AttachmentIds { get; set; } = [];

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>Thread root message id, or null for a top-level draft.</summary>
    [Map("parent_id")]
    public int? ParentId { get; set; }

    public static DraftMessage FromDto(DraftMessageDto message, Guid userId) =>
        new()
        {
            ChannelId = message.ChannelId,
            UserId = userId,
            Content = message.Content,
            AttachmentIds = message.AttachmentIds ?? [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ParentId = message.ParentId,
        };

    public static Expression<Func<DraftMessage, bool>> MatchByKey(int key) => message => message.Id == key;

    public static int GetKey(DraftMessage record) => record.Id;
}

/// <summary>Input for saving a draft.</summary>
public class DraftMessageDto
{
    public required int ChannelId { get; set; }

    [MaxLength(4096)]
    public required string Content { get; set; }

    public int[]? AttachmentIds { get; set; }

    public int? ParentId { get; set; }
}
