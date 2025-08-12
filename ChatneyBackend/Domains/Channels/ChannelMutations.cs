using MongoDB.Driver;

namespace ChatneyBackend.Domains.Channels;

public class ChannelMutations
{
    public Channel AddChannel(IMongoDatabase mongoDatabase, ChannelDTO channelDto)
    {
        var collection = mongoDatabase.GetCollection<Channel>("channels");
        Channel channel = Channel.FromDTO(channelDto);
        collection.InsertOne(channel);
        return channel;
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

    public bool DeleteChannelGroup(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>("channel_groups");
        var result = collection.DeleteOne(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
