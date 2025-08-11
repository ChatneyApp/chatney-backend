using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class ChannelGroup
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [BsonElement("workspaceId")]
    [MaxLength(36)]
    public string WorkspaceId { get; set; }

    [BsonElement("channelIds")]
    public string[] ChannelIds { get; set; }

    [BsonElement("order")]
    public int Order { get; set; }
}
