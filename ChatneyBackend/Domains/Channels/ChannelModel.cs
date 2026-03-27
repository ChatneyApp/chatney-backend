using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class Channel : DatabaseItem
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
    public int WorkspaceId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public class ChannelDto : IDTO<Channel>
{
    [BsonElement("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [BsonElement("channelTypeId")]
    [MaxLength(36)]
    public required string ChannelTypeId { get; set; }

    [BsonElement("workspaceId")]
    public int WorkspaceId { get; set; }

    public Channel ToModel()
    {
        return new Channel()
        {
            Id = Guid.NewGuid().ToString(),
            Name = Name,
            WorkspaceId = WorkspaceId,
            ChannelTypeId = ChannelTypeId,
        };
    }
}
