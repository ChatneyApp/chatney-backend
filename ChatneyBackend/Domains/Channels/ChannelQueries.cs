using ChatneyBackend.Models;

namespace ChatneyBackend.Domains.Channels;

[ExtendObjectType("Query")]
public class ChannelQueries
{
    public string HelloChannel() => "Hello from GraphQL!";

    public Channel GetChannelById(ApplicationDbContext dbContext, string id)
        => dbContext.Channels.First(u => u.Id == id);

    public Channel GetChannelByName(ApplicationDbContext dbContext, string name)
        => dbContext.Channels.First(u => u.Name == name);

    public IQueryable<Channel> GetChannels(ApplicationDbContext dbContext)
        => dbContext.Channels;
}
