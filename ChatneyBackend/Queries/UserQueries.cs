using ChatneyBackend.Models;

namespace ChatneyBackend.Queries;

public class UserQueries
{
    public string Hello() => "Hello from GraphQL!";

    public User GetUserById(ApplicationDbContext dbContext, string id)
        => dbContext.Users.First(u => u.Id == id);

    public User GetUserByName(ApplicationDbContext dbContext, string name)
        => dbContext.Users.First(u => u.Name == name);

    public IQueryable<User> GetUsers(ApplicationDbContext dbContext)
        => dbContext.Users;
}
