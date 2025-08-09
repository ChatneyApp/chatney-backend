using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Configs;

public class Config
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("key")]
    [MaxLength(255)]
    public required string Key { get; set; }

    [BsonElement("value")]
    [MaxLength(2048)]
    public required string Value { get; set; }

    [BsonElement("workspaceId")]
    [MaxLength(36)]
    public string? WorkspaceId { get; set; }
} 