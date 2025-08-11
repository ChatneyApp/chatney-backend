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

    [BsonElement("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [BsonElement("value")]
    [MaxLength(2048)]
    public required string Value { get; set; }

    [BsonElement("type")]
    [MaxLength(255)]
    public string? Type { get; set; }
} 
