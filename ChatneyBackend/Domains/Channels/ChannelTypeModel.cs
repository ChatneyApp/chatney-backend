using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class ChannelType : IPgKey<ChannelType, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Map("key")]
    [MaxLength(255)]
    public required string Key { get; set; }

    [Map("sec_obj_id")]
    public int SecObjId { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static ChannelType FromDto(ChannelTypeDto channelType)
    {
        return new ChannelType
        {
            Name = channelType.Name,
            Key = channelType.Key,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public static Expression<Func<ChannelType, bool>> MatchByKey(int key) => channelType => channelType.Id == key;

    public static int GetKey(ChannelType record) => record.Id;
}

public class ChannelTypeDto
{
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public required string Key { get; set; }
}

public class WebsocketChannelTypePayload
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Key { get; set; }

    public static WebsocketChannelTypePayload FromChannelType(ChannelType channelType) => new()
    {
        Id = channelType.Id,
        Name = channelType.Name,
        Key = channelType.Key,
    };
}

public class WebsocketChannelTypeDeletedPayload
{
    public int Id { get; set; }
}
