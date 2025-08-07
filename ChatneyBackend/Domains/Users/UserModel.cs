using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Users;

public class User
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [BsonElement("email")]
    [MaxLength(255)]
    public required string Email { get; set; }
}
