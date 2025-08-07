using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class Channel
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public required string Id { get; set; }

    [BsonElement("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [BsonElement("channelTypeId")]
    [MaxLength(36)]
    public required string ChannelTypeId { get; set; }

    [BsonElement("workspaceId")]
    [MaxLength(36)]
    public required string WorkspaceId { get; set; }
}
