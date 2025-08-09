using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Messages;

public class Message
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("content")]
    [MaxLength(4096)]
    public required string Content { get; set; }

    [BsonElement("channelId")]
    [MaxLength(36)]
    public required string ChannelId { get; set; }

    [BsonElement("userId")]
    [MaxLength(36)]
    public required string UserId { get; set; }
} 