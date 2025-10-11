using System.Diagnostics;
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

    // TODO: implement side query to get all the users & attachments involved
    public async Task<List<Message>> GetListChannelMessages(Repo<Message> repo, string channelId) =>
        await repo.GetList(
            Builders<Message>.Filter.Eq(m => m.ChannelId, channelId)
        );

    public async Task<List<MessageUser>> GetMessageUsers(Message message)
    {
        Debug.WriteLine(message.Id);

        return new List<MessageUser>();
    }
}
