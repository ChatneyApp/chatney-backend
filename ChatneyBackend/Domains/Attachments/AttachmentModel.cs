using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Domains.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Attachments;

public class Attachment : DatabaseItem, IHasUserId
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    [MaxLength(36)]
    public required Guid UserId { get; set; }

    /// <summary>
    /// Path within the bucket: /media/{userId}/{date::YYYY-MM-DD}/{fileId}.{extension}
    /// This can be used to determine how to display the attachment in the frontend.
    /// </summary>
    [BsonElement("urlPath")]
    [MaxLength(4096)]
    public required string UrlPath { get; set; }

    [BsonElement("originalFileName")]
    [MaxLength(4096)]
    public required string OriginalFileName { get; set; }

    [BsonElement("extension")]
    [MaxLength(4096)]
    public required string Extension { get; set; }

    [BsonElement("mimeType")]
    [MaxLength(4096)]
    public required string MimeType { get; set; }

    /// <summary>
    /// The type of the attachment, e.g., "image", "video", "file", etc.
    /// This can be used to determine how to display the attachment in the frontend.
    /// </summary>
    [BsonElement("type")]
    [MaxLength(4096)]
    public required string Type { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
