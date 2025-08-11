using MongoDB.Driver;

namespace ChatneyBackend.Domains.Channels;

public class ChannelMutations
{
    public Channel AddChannel(IMongoDatabase mongoDatabase, Channel channel)
    {
        var collection = mongoDatabase.GetCollection<Channel>("channels");
        collection.InsertOne(channel);
        return channel;
    }

    public bool DeleteChannel(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Channel>("channels");
        var result = collection.DeleteOne(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
