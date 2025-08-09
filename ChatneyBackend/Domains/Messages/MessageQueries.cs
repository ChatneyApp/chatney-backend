using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Messages;

[ExtendObjectType("Query")]
public class MessageQueries
{
    public Message GetMessageById(ApplicationDbContext dbContext, string id)
        => dbContext.Messages.First(m => m.Id == id);

    public IQueryable<Message> GetMessages(ApplicationDbContext dbContext)
        => dbContext.Messages;

    public IQueryable<Message> GetMessagesByChannelId(ApplicationDbContext dbContext, string channelId)
        => dbContext.Messages.Where(m => m.ChannelId == channelId);
} 
