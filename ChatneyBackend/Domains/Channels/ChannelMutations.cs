using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Channels;

public class ChannelMutations
{
    public async Task<ChannelType> AddChannelType(AppRepos repos, ChannelTypeDto channelTypeDto)
    {
        var channelType = ChannelType.FromDto(channelTypeDto);
        channelType.Id = await repos.ChannelTypes.InsertOne(channelType);
        return channelType;
    }

    public async Task<ChannelType?> UpdateChannelType(AppRepos repos, ChannelType channelType)
    {
        var updated = await repos.ChannelTypes.UpdateOne(channelType);
        return updated ? channelType : null;
    }

    public async Task<bool> DeleteChannelType(AppRepos repos, int id) =>
        await repos.ChannelTypes.DeleteById(id);

    public async Task<Channel> AddChannel(AppRepos repos, ChannelDto channelDto)
    {
        var channel = channelDto.ToModel();
        channel.Id = await repos.Channels.InsertOne(channel);
        return channel;
    }

    public async Task<Channel?> UpdateChannel(AppRepos repos, Channel channel)
    {
        var updated = await repos.Channels.UpdateOne(channel);
        return updated ? channel : null;
    }

    public async Task<bool> DeleteChannel(AppRepos repos, int id) => await repos.Channels.DeleteById(id);

    public async Task<ChannelGroup> AddChannelGroup(AppRepos repos, ChannelGroupDto channelGroupDto)
    {
        var channelGroup = ChannelGroup.FromDto(channelGroupDto);
        channelGroup.Id = await repos.ChannelGroups.InsertOne(channelGroup);
        return channelGroup;
    }

    public async Task<ChannelGroup?> UpdateChannelGroup(AppRepos repos, ChannelGroup channelGroup)
    {
        var updated = await repos.ChannelGroups.UpdateOne(channelGroup);
        return updated ? channelGroup : null;
    }

    public async Task<bool> DeleteChannelGroup(AppRepos repos, int id) =>
        await repos.ChannelGroups.DeleteById(id);
}
