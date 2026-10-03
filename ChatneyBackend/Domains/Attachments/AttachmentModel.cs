using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Attachments;

/// <summary>An uploaded file's metadata.</summary>
public class Attachment : IPgKey<Attachment, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    /// <summary>Uploader.</summary>
    [Map("user_id")]
    public required Guid UserId { get; set; }

    /// <summary>
    /// Object key within the storage bucket: attachments/{userId}/{yyyy-MM-dd}/{fileId}.{extension}
    /// </summary>
    [Map("url_path")]
    [MaxLength(4096)]
    public required string UrlPath { get; set; }

    /// <summary>File name as uploaded by the user.</summary>
    [Map("original_file_name")]
    [MaxLength(4096)]
    public required string OriginalFileName { get; set; }

    /// <summary>Stored file extension without the dot. Audio is stored as mp3, video as mp4. May be empty.</summary>
    [Map("extension")]
    [MaxLength(4096)]
    public required string Extension { get; set; }

    [Map("mime_type")]
    [MaxLength(4096)]
    public required string MimeType { get; set; }

    /// <summary>File size in bytes.</summary>
    [Map("size")]
    public required long Size { get; set; }

    /// <summary>Media width in pixels, as reported by the uploader.</summary>
    [Map("width")]
    public int? Width { get; set; }

    /// <summary>Media height in pixels, as reported by the uploader.</summary>
    [Map("height")]
    public int? Height { get; set; }

    /// <summary>Audio/video duration in seconds, as reported by the uploader.</summary>
    [Map("duration")]
    public int? Duration { get; set; }

    /// <summary>
    /// Display kind derived from the MIME type: "image", "gif", "video", "audio" or "binary".
    /// This can be used to determine how to display the attachment in the frontend.
    /// </summary>
    [Map("type")]
    [MaxLength(4096)]
    public required string Type { get; set; }

    /// <summary>Show as a downloadable file rather than inline media.</summary>
    [Map("as_file")]
    public bool AsFile { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static Expression<Func<Attachment, bool>> MatchByKey(int key) => attachment => attachment.Id == key;

    public static int GetKey(Attachment record) => record.Id;
}
