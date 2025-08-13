using MongoDB.Driver;

namespace ChatneyBackend.Domains.Channels;

public class ChannelMutations
{
    public ChannelType AddChannelType(IMongoDatabase mongoDatabase, ChannelTypeDTO channelTypeDto)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>("channel_types");
        ChannelType channelType = ChannelType.FromDTO(channelTypeDto);
        collection.InsertOne(channelType);
        return channelType;
    }

    public async Task<ChannelType> UpdateChannelType(IMongoDatabase mongoDatabase, ChannelType channelType)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>("channel_types");
        var filter = Builders<ChannelType>.Filter.Eq("_id", channelType.Id);
        var result = await collection.ReplaceOneAsync(filter, channelType);
        return result.IsAcknowledged ? channelType : null;
    }

    public bool DeleteChannelType(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>("channel_types");
        var result = collection.DeleteOne(c => c.Id == id);
        return result.DeletedCount > 0;
    }

    public Channel AddChannel(IMongoDatabase mongoDatabase, ChannelDTO channelDto)
    {
        var collection = mongoDatabase.GetCollection<Channel>("channels");
        Channel channel = Channel.FromDTO(channelDto);
        collection.InsertOne(channel);
        return channel;
    }

    public async Task<Channel> UpdateChannel(IMongoDatabase mongoDatabase, Channel channel)
    {
        var collection = mongoDatabase.GetCollection<Channel>("channels");
        var filter = Builders<Channel>.Filter.Eq("_id", channel.Id);
        var result = await collection.ReplaceOneAsync(filter, channel);
        return result.IsAcknowledged ? channel : null;
    }

    public bool DeleteChannel(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Channel>("channels");
        var result = collection.DeleteOne(c => c.Id == id);
        return result.DeletedCount > 0;
    }

    public ChannelGroup AddChannelGroup(IMongoDatabase mongoDatabase, ChannelGroupDTO channelGroupDto)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>("channel_groups");
        ChannelGroup channelGroup = ChannelGroup.FromDTO(channelGroupDto);
        collection.InsertOne(channelGroup);
        return channelGroup;
    }

    public async Task<ChannelGroup> UpdateChannelGroup(IMongoDatabase mongoDatabase, ChannelGroup channelGroup)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>("channel_groups");
        var filter = Builders<ChannelGroup>.Filter.Eq("_id", channelGroup.Id);
        var result = await collection.ReplaceOneAsync(filter, channelGroup);
        return result.IsAcknowledged ? channelGroup : null;
    }

    public bool DeleteChannelGroup(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>("channel_groups");
        var result = collection.DeleteOne(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
