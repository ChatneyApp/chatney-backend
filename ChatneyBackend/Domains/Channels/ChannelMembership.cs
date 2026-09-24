using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Channels;

public static class ChannelMembership
{
    public static HashSet<Guid> DistinctMemberIds(Guid actorId, IEnumerable<Guid> otherUserIds)
    {
        var memberIds = otherUserIds
            .Where(id => id != actorId)
            .ToHashSet();
        memberIds.Add(actorId);
        return memberIds;
    }

    public static async Task<List<ChannelMember>> ListForChannel(AppRepos repos, int channelId) =>
        await repos.ChannelMembers.GetList(member => member.ChannelId == channelId);

    public static async Task<List<Guid>> UserIdsForChannel(AppRepos repos, int channelId)
    {
        var members = await ListForChannel(repos, channelId);
        return members.Select(member => member.UserId).ToList();
    }

    public static async Task<IReadOnlyList<Guid>?> FanoutUserIds(AppRepos repos, Channel channel)
    {
        if (!channel.IsDm)
        {
            return null;
        }

        return await UserIdsForChannel(repos, channel.Id);
    }

    public static async Task<Channel?> FindByExactMembers(
        AppRepos repos,
        int dmChannelTypeId,
        HashSet<Guid> memberIds)
    {
        var seedUserId = memberIds.First();
        var seedMemberships = await repos.ChannelMembers.GetList(member => member.UserId == seedUserId);

        foreach (var membership in seedMemberships)
        {
            var channel = await repos.Channels.GetById(membership.ChannelId);
            if (channel == null || !channel.IsDm || channel.ChannelTypeId != dmChannelTypeId)
            {
                continue;
            }

            var channelMembers = await ListForChannel(repos, channel.Id);
            if (channelMembers.Count == memberIds.Count &&
                channelMembers.All(member => memberIds.Contains(member.UserId)))
            {
                return channel;
            }
        }

        return null;
    }

    public static async Task InsertMembers(AppRepos repos, int channelId, IEnumerable<Guid> userIds)
    {
        var now = DateTime.UtcNow;
        await repos.ChannelMembers.InsertBulk(userIds
            .Select(userId => new ChannelMember
            {
                ChannelId = channelId,
                UserId = userId,
                CreatedAt = now,
            })
            .ToList());
    }

    public static async Task<DirectMessage> ToDirectMessage(
        AppRepos repos,
        Channel channel,
        Guid actorId)
    {
        var memberIds = await UserIdsForChannel(repos, channel.Id);
        var otherUserIds = memberIds.Where(id => id != actorId).ToList();
        var otherUsers = otherUserIds.Count == 0
            ? []
            : await repos.Users.GetList(user => otherUserIds.Contains(user.Id));

        return DirectMessage.From(channel, otherUsers.OrderBy(user => user.Nickname));
    }
}
