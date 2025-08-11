using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    public Message GetMessageById(IMongoDatabase mongoDatabase, string id)
        => mongoDatabase.GetCollection<Message>("messages").Find(m => m.Id == id).First();

    public List<Message> GetList(IMongoDatabase mongoDatabase, string channelId)
        => mongoDatabase.GetCollection<Message>("messages").Find(m => m.ChannelId == channelId).ToList();
}
