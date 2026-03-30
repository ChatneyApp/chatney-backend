using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Channels;

public class ChannelMutations
{
    public async Task<ChannelType> AddChannelType(PgRepo<ChannelType, int> channelTypeRepo, ChannelTypeDto channelTypeDto)
    {
        var channelType = ChannelType.FromDto(channelTypeDto);
        channelType.Id = await channelTypeRepo.InsertOne(channelType);
        return channelType;
    }

    public async Task<ChannelType?> UpdateChannelType(PgRepo<ChannelType, int> channelTypeRepo, ChannelType channelType)
    {
        var updated = await channelTypeRepo.UpdateOne(channelType);
        return updated ? channelType : null;
    }

    public async Task<bool> DeleteChannelType(PgRepo<ChannelType, int> channelTypeRepo, int id) =>
        await channelTypeRepo.DeleteById(id);

    public async Task<Channel> AddChannel(PgRepo<Channel, int> channelRepo, ChannelDto channelDto)
    {
        var channel = channelDto.ToModel();
        channel.Id = await channelRepo.InsertOne(channel);
        return channel;
    }

    public async Task<Channel?> UpdateChannel(PgRepo<Channel, int> channelRepo, Channel channel)
    {
        var updated = await channelRepo.UpdateOne(channel);
        return updated ? channel : null;
    }

    public async Task<bool> DeleteChannel(PgRepo<Channel, int> channelRepo, int id) => await channelRepo.DeleteById(id);

    public async Task<ChannelGroup> AddChannelGroup(PgRepo<ChannelGroup, int> channelGroupRepo, ChannelGroupDto channelGroupDto)
    {
        var channelGroup = ChannelGroup.FromDto(channelGroupDto);
        channelGroup.Id = await channelGroupRepo.InsertOne(channelGroup);
        return channelGroup;
    }

    public async Task<ChannelGroup?> UpdateChannelGroup(PgRepo<ChannelGroup, int> channelGroupRepo, ChannelGroup channelGroup)
    {
        var updated = await channelGroupRepo.UpdateOne(channelGroup);
        return updated ? channelGroup : null;
    }

    public async Task<bool> DeleteChannelGroup(PgRepo<ChannelGroup, int> channelGroupRepo, int id) =>
        await channelGroupRepo.DeleteById(id);
}
