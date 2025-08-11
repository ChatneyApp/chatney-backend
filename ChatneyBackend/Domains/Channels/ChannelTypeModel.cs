using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class ChannelType
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("label")]
    [MaxLength(255)]
    public required string Label { get; set; }

    [BsonElement("key")]
    [MaxLength(255)]
    public required string Key { get; set; }

    [BsonElement("baseRoleId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string BaseRoleId { get; set; }

}
