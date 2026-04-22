using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Attachments;

public class Attachment : IPgKey<Attachment, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("user_id")]
    public required Guid UserId { get; set; }

    /// <summary>
    /// Path within the bucket: /media/{userId}/{date::YYYY-MM-DD}/{fileId}.{extension}
    /// This can be used to determine how to display the attachment in the frontend.
    /// </summary>
    [Map("url_path")]
    [MaxLength(4096)]
    public required string UrlPath { get; set; }

    [Map("original_file_name")]
    [MaxLength(4096)]
    public required string OriginalFileName { get; set; }

    [Map("extension")]
    [MaxLength(4096)]
    public required string Extension { get; set; }

    [Map("mime_type")]
    [MaxLength(4096)]
    public required string MimeType { get; set; }

    /// <summary>
    /// The type of the attachment, e.g., "image", "video", "file", etc.
    /// This can be used to determine how to display the attachment in the frontend.
    /// </summary>
    [Map("type")]
    [MaxLength(4096)]
    public required string Type { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static Expression<Func<Attachment, bool>> MatchByKey(int key) => attachment => attachment.Id == key;
}
