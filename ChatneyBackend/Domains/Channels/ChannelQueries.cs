using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Channels;

public class ChannelQueries
{
    public Channel GetChannelById(ApplicationDbContext dbContext, string id)
        => dbContext.Channels.First(u => u.Id == id);

    public Channel GetChannelByName(ApplicationDbContext dbContext, string name)
        => dbContext.Channels.First(u => u.Name == name);

    public IQueryable<Channel> GetList(ApplicationDbContext dbContext)
        => dbContext.Channels;
}
