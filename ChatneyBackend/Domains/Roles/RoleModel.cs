using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Roles;

public class Role
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [BsonElement("description")]
    [MaxLength(1024)]
    public string? Description { get; set; }
} 