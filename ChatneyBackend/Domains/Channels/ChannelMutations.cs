using MongoDB.Driver;

namespace ChatneyBackend.Domains.Channels;

public class ChannelMutations
{
    public async Task<ChannelType> AddChannelType(IMongoDatabase mongoDatabase, ChannelTypeDTO channelTypeDto)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>("channel_types");
        ChannelType channelType = ChannelType.FromDTO(channelTypeDto);
        await collection.InsertOneAsync(channelType);
        return channelType;
    }

    public async Task<ChannelType?> UpdateChannelType(IMongoDatabase mongoDatabase, ChannelType channelType)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>("channel_types");
        var filter = Builders<ChannelType>.Filter.Eq("_id", channelType.Id);
        var result = await collection.ReplaceOneAsync(filter, channelType);
        return result.ModifiedCount > 0 ? channelType : null;
    }

    public async Task<bool> DeleteChannelType(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>("channel_types");
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<Channel> AddChannel(IMongoDatabase mongoDatabase, ChannelDTO channelDto)
    {
        var collection = mongoDatabase.GetCollection<Channel>("channels");
        Channel channel = Channel.FromDTO(channelDto);
        await collection.InsertOneAsync(channel);
        return channel;
    }

    public async Task<Channel?> UpdateChannel(IMongoDatabase mongoDatabase, Channel channel)
    {
        var collection = mongoDatabase.GetCollection<Channel>("channels");
        var filter = Builders<Channel>.Filter.Eq("_id", channel.Id);
        var result = await collection.ReplaceOneAsync(filter, channel);
        return result.ModifiedCount > 0 ? channel : null;
    }

    public async Task<bool> DeleteChannel(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Channel>("channels");
        var result = collection.DeleteOne(c => c.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<ChannelGroup> AddChannelGroup(IMongoDatabase mongoDatabase, ChannelGroupDTO channelGroupDto)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>("channel_groups");
        ChannelGroup channelGroup = ChannelGroup.FromDTO(channelGroupDto);
        await collection.InsertOneAsync(channelGroup);
        return channelGroup;
    }

    public async Task<ChannelGroup?> UpdateChannelGroup(IMongoDatabase mongoDatabase, ChannelGroup channelGroup)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>("channel_groups");
        var filter = Builders<ChannelGroup>.Filter.Eq("_id", channelGroup.Id);
        var result = await collection.ReplaceOneAsync(filter, channelGroup);
        return result.ModifiedCount > 0 ? channelGroup : null;
    }

    public async Task<bool> DeleteChannelGroup(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>("channel_groups");
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
