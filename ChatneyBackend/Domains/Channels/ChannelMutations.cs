using System.Diagnostics;
using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Channels;

[ExtendObjectType("Mutation")]
public class ChannelMutations
{
    public Channel AddChannel(ApplicationDbContext dbContext, Channel channel)
    {
        dbContext.Channels.Add(channel);
        try
        {
            dbContext.SaveChanges();
            return channel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return null;
    }

    public bool DeleteChannel(ApplicationDbContext dbContext, string id)
    {
        var user = dbContext.Channels.First(user => user.Id == id);

        try
        {
            dbContext.Channels.Remove(user);
            dbContext.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return false;
    }
}
