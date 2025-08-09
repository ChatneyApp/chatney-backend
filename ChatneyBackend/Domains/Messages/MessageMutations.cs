using System.Diagnostics;
using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Messages;

[ExtendObjectType("Mutation")]
public class MessageMutations
{
    public Message AddMessage(ApplicationDbContext dbContext, Message message)
    {
        dbContext.Messages.Add(message);
        try
        {
            dbContext.SaveChanges();
            return message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return null;
    }

    public bool DeleteMessage(ApplicationDbContext dbContext, string id)
    {
        var message = dbContext.Messages.First(message => message.Id == id);

        try
        {
            dbContext.Messages.Remove(message);
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