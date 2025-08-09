using ChatneyBackend.Models;

namespace ChatneyBackend.Domains.Users;

[ExtendObjectType("Query")]
public class UserQueries
{
    public User GetUserById(ApplicationDbContext dbContext, string id)
        => dbContext.Users.First(u => u.Id == id);

    public User GetUserByName(ApplicationDbContext dbContext, string name)
        => dbContext.Users.First(u => u.Name == name);

    public IQueryable<User> GetUsers(ApplicationDbContext dbContext)
        => dbContext.Users;
}
