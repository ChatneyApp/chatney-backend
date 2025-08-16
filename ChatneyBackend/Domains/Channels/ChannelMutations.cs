using MongoDB.Driver;

namespace ChatneyBackend.Domains.Channels;

public class ChannelMutations
{
    public async Task<ChannelType> AddChannelType(IMongoDatabase mongoDatabase, ChannelTypeDTO channelTypeDto)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>(DomainSettings.ChannelTypeCollectionName);
        ChannelType channelType = ChannelType.FromDTO(channelTypeDto);
        await collection.InsertOneAsync(channelType);
        return channelType;
    }

    public async Task<ChannelType?> UpdateChannelType(IMongoDatabase mongoDatabase, ChannelType channelType)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>(DomainSettings.ChannelTypeCollectionName);
        var filter = Builders<ChannelType>.Filter.Eq("_id", channelType.Id);
        var result = await collection.ReplaceOneAsync(filter, channelType);
        return result.ModifiedCount > 0 ? channelType : null;
    }

    public async Task<bool> DeleteChannelType(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>(DomainSettings.ChannelTypeCollectionName);
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<Channel> AddChannel(IMongoDatabase mongoDatabase, ChannelDTO channelDto)
    {
        var collection = mongoDatabase.GetCollection<Channel>(DomainSettings.ChannelCollectionName);
        Channel channel = Channel.FromDTO(channelDto);
        await collection.InsertOneAsync(channel);
        return channel;
    }

    public async Task<Channel?> UpdateChannel(IMongoDatabase mongoDatabase, Channel channel)
    {
        var collection = mongoDatabase.GetCollection<Channel>(DomainSettings.ChannelCollectionName);
        var filter = Builders<Channel>.Filter.Eq("_id", channel.Id);
        var result = await collection.ReplaceOneAsync(filter, channel);
        return result.ModifiedCount > 0 ? channel : null;
    }

    public async Task<bool> DeleteChannel(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Channel>(DomainSettings.ChannelCollectionName);
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<ChannelGroup> AddChannelGroup(IMongoDatabase mongoDatabase, ChannelGroupDTO channelGroupDto)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>(DomainSettings.ChannelGroupCollectionName);
        ChannelGroup channelGroup = ChannelGroup.FromDTO(channelGroupDto);
        await collection.InsertOneAsync(channelGroup);
        return channelGroup;
    }

    public async Task<ChannelGroup?> UpdateChannelGroup(IMongoDatabase mongoDatabase, ChannelGroup channelGroup)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>(DomainSettings.ChannelGroupCollectionName);
        var filter = Builders<ChannelGroup>.Filter.Eq("_id", channelGroup.Id);
        var result = await collection.ReplaceOneAsync(filter, channelGroup);
        return result.ModifiedCount > 0 ? channelGroup : null;
    }

    public async Task<bool> DeleteChannelGroup(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>(DomainSettings.ChannelGroupCollectionName);
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
