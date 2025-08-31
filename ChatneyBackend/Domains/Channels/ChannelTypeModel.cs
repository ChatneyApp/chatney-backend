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

    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }

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
            Name = channelType.Name,
            Key = channelType.Key,
            BaseRoleId = channelType.BaseRoleId,
        };
    }
}

public class ChannelTypeDTO
{
    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [BsonElement("key")]
    [MaxLength(255)]
    public string Key { get; set; }

    [BsonElement("baseRoleId")]
    [MaxLength(36)]
    public string BaseRoleId { get; set; }

}
