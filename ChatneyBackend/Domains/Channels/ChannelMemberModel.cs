using System.Linq.Expressions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Channels;

public readonly record struct ChannelMemberKey(int ChannelId, Guid UserId);

public class ChannelMember : IPgKey<ChannelMember, ChannelMemberKey>
{
    [Primary]
    [Map("channel_id")]
    public required int ChannelId { get; set; }

    [Primary]
    [Map("user_id")]
    public required Guid UserId { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    public static Expression<Func<ChannelMember, bool>> MatchByKey(ChannelMemberKey key) =>
        member => member.ChannelId == key.ChannelId && member.UserId == key.UserId;

    public static ChannelMemberKey GetKey(ChannelMember record) => new(record.ChannelId, record.UserId);
}
