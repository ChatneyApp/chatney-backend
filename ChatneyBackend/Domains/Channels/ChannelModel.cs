using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class Channel
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [BsonElement("channelTypeId")]
    [MaxLength(36)]
    public string ChannelTypeId { get; set; }

    [BsonElement("workspaceId")]
    [MaxLength(36)]
    public string WorkspaceId { get; set; }

    public static Channel FromDTO(ChannelDTO channel)
    {
        return new Channel()
        {
            Id = Guid.NewGuid().ToString(),
            Name = channel.Name,
            WorkspaceId = channel.WorkspaceId,
            ChannelTypeId = channel.ChannelTypeId,
        };
    }
}

public class ChannelDTO
{
    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [BsonElement("channelTypeId")]
    [MaxLength(36)]
    public string ChannelTypeId { get; set; }

    [BsonElement("workspaceId")]
    [MaxLength(36)]
    public string WorkspaceId { get; set; }
}
