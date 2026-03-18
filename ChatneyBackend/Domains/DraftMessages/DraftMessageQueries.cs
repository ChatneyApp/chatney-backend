using HotChocolate.Authorization;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessageQueries
{
    [Authorize]
    public async Task<DraftMessage?> GetMessageById(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<DraftMessage>(DomainSettings.MessageCollectionName);
        var records = (await collection.FindAsync(r => r.Id == id)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    [Authorize]
    public async Task<List<DraftMessage>> GetListChannelMessages(Repo<DraftMessage> repo, string channelId) =>
        await repo.GetList(
            Builders<DraftMessage>.Filter.Eq(m => m.ChannelId, channelId) &
            Builders<DraftMessage>.Filter.Eq(m => m.ParentId, null)
        );

    [Authorize]
    public async Task<List<DraftMessage>> GetListThreadMessages(Repo<DraftMessage> repo, string threadId) =>
        await repo.GetList(
            Builders<DraftMessage>.Filter.Eq(m => m.ParentId, threadId)
        );
}
