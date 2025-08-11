using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Messages;

public class Reaction
{
    [BsonElement("userId")]
    [MaxLength(36)]
    public required string UserId { get; set; }

    [BsonElement("reaction")]
    [MaxLength(255)]
    public required string ReactionText { get; set; }
}

public class Message
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("channelId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string ChannelId { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string UserId { get; set; }

    [BsonElement("content")]
    [MaxLength(4096)]
    public required string Content { get; set; }

    [BsonElement("attachments")]
    public required List<string> Attachments { get; set; }

    [BsonElement("status")]
    [MaxLength(50)]
    public required string Status { get; set; }

    [BsonElement("createdAt")]
    public required DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public required DateTime UpdatedAt { get; set; }

    [BsonElement("reactions")]
    public required List<Reaction> Reactions { get; set; }

    [BsonElement("parentId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public string? ParentId { get; set; }
}
