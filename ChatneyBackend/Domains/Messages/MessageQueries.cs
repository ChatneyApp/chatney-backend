using HotChocolate.Authorization;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    [Authorize]
    public async Task<Message?> GetMessageById(PgRepo<Message, int> repo, int id) => await repo.GetById(id);

    [Authorize]
    public async Task<List<Message>> GetListChannelMessages(PgRepo<Message, int> repo, int channelId) =>
        await repo.GetList(m => m.ChannelId == channelId && m.ParentId == null);

    [Authorize]
    public async Task<List<Message>> GetListThreadMessages(PgRepo<Message, int> repo, int threadId) =>
        await repo.GetList(m => m.ParentId == threadId);
}
