using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    public async Task<Message?> GetMessageById(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
        var records = (await collection.FindAsync(r => r.Id == id)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    public async Task<List<Message>> GetListChannelMessages(IMongoDatabase mongoDatabase, string channelId)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
        var records = await collection.FindAsync(r => r.ChannelId == channelId);
        return records.ToList();

    }
}
