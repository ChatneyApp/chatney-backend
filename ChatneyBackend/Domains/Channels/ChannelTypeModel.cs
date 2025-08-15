using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class ChannelType
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("label")]
    [MaxLength(255)]
    public string Label { get; set; }

    [BsonElement("key")]
    [MaxLength(255)]
    public string Key { get; set; }

    [BsonElement("baseRoleId")]
    [MaxLength(36)]
    public string BaseRoleId { get; set; }

    public static ChannelType FromDTO(ChannelTypeDTO channelType)
    {
        return new ChannelType()
        {
            Id = Guid.NewGuid().ToString(),
            Label = channelType.Label,
            Key = channelType.Key,
            BaseRoleId = channelType.BaseRoleId,
        };
    }
}

public class ChannelTypeDTO
{
    [BsonElement("label")]
    [MaxLength(255)]
    public string Label { get; set; }

    [BsonElement("key")]
    [MaxLength(255)]
    public string Key { get; set; }

    [BsonElement("baseRoleId")]
    [MaxLength(36)]
    public string BaseRoleId { get; set; }

}
