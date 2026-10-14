using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Channels;

/// <summary>A named, ordered sidebar section of channels within a workspace. It doesn't affect permissions.</summary>
public class ChannelGroup : IPgKey<ChannelGroup, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Map("workspace_id")]
    public int WorkspaceId { get; set; }

    /// <summary>Channels in this group, in display order.</summary>
    [Map("channel_ids")]
    public required int[] ChannelIds { get; set; }

    /// <summary>Sort position among the workspace's groups.</summary>
    [Map("order")]
    public int Order { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static ChannelGroup FromDto(ChannelGroupDto channelGroup)
    {
        return new ChannelGroup
        {
            Name = channelGroup.Name,
            WorkspaceId = channelGroup.WorkspaceId,
            ChannelIds = channelGroup.ChannelIds,
            Order = channelGroup.Order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public static Expression<Func<ChannelGroup, bool>> MatchByKey(int key) => group => group.Id == key;

    public static int GetKey(ChannelGroup record) => record.Id;
}

/// <summary>Input for creating a channel group.</summary>
public class ChannelGroupDto
{
    [MaxLength(255)]
    public required string Name { get; set; }

    public int WorkspaceId { get; set; }

    public required int[] ChannelIds { get; set; }

    public int Order { get; set; }
}
