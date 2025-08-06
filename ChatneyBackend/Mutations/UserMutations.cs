using System.Diagnostics;
using ChatneyBackend.Models;

namespace ChatneyBackend.Mutations;

public class UserMutations
{
    public User AddUser(ApplicationDbContext dbContext, User user)
    {
        dbContext.Users.Add(user);
        try
        {
            dbContext.SaveChanges();
            return user;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return null;
    }

    public bool DeleteUser(ApplicationDbContext dbContext, string id)
    {
        var user = dbContext.Users.First(user => user.Id == id);

        try
        {
            dbContext.Users.Remove(user);
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
