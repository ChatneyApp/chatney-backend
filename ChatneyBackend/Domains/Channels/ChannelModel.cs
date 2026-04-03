using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Infra;
using ChatneyBackend.Utils;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Channels;

public class Channel : IPgKey<Channel, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Map("channel_type_id")]
    public int ChannelTypeId { get; set; }

    [Map("workspace_id")]
    public int WorkspaceId { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static Channel FromDto(ChannelDto channel)
    {
        return new Channel
        {
            Name = channel.Name,
            WorkspaceId = channel.WorkspaceId,
            ChannelTypeId = channel.ChannelTypeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public static Expression<Func<Channel, bool>> MatchByKey(int key) => channel => channel.Id == key;
}

public class ChannelDto : IDto<Channel>
{
    [MaxLength(255)]
    public required string Name { get; set; }

    public int ChannelTypeId { get; set; }

    public int WorkspaceId { get; set; }

    public Channel ToModel() => Channel.FromDto(this);
}
