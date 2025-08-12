using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class ChannelGroup
{
    [BsonElement("_id")]
    [BsonId]
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

    public static ChannelGroup FromDTO(ChannelGroupDTO channelGroup)
    {
        return new ChannelGroup()
        {
            Id = Guid.NewGuid().ToString(),
            Name = channelGroup.Name,
            WorkspaceId = channelGroup.WorkspaceId,
            ChannelIds = channelGroup.ChannelIds,
            Order = channelGroup.Order
        };
    }
}

public class ChannelGroupDTO
{
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
