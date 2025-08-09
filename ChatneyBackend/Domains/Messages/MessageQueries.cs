using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    public Message GetMessageById(ApplicationDbContext dbContext, string id)
        => dbContext.Messages.First(m => m.Id == id);

    public IQueryable<Message> GetList(ApplicationDbContext dbContext, string channelId)
        => dbContext.Messages.Where(m => m.ChannelId == channelId);
} 
