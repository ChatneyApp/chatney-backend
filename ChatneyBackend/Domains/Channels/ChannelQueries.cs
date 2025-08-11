using MongoDB.Driver;

namespace ChatneyBackend.Domains.Channels;

public class ChannelQueries
{
    public Channel GetChannelById(IMongoDatabase mongoDatabase, string id)
        => mongoDatabase.GetCollection<Channel>("channels").Find(u => u.Id == id).First();

    public Channel GetChannelByName(IMongoDatabase mongoDatabase, string name)
        => mongoDatabase.GetCollection<Channel>("channels").Find(u => u.Name == name).First();

    public List<Channel> GetList(IMongoDatabase mongoDatabase)
        => mongoDatabase.GetCollection<Channel>("channels").Find(Builders<Channel>.Filter.Empty).ToList();
}
