using HotChocolate.Authorization;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    [Authorize]
    public async Task<Message?> GetMessageById(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
        var records = (await collection.FindAsync(r => r.Id == id)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    [Authorize]
    public async Task<List<Message>> GetListChannelMessages(Repo<Message> repo, int channelId)
    {
        try
        {
            var result = await repo.GetList(
                Builders<Message>.Filter.Eq(m => m.ChannelId, channelId) &
                Builders<Message>.Filter.Eq(m => m.ParentId, null)
            );
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new List<Message>();
        }
    }

    [Authorize]
    public async Task<List<Message>> GetListThreadMessages(Repo<Message> repo, string threadId) =>
        await repo.GetList(
            Builders<Message>.Filter.Eq(m => m.ParentId, threadId)
        );
}
