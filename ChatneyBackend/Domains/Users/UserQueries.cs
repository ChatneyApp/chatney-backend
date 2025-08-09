using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Users;

public class UserQueries
{
    public User GetUserById(ApplicationDbContext dbContext, string id)
        => dbContext.Users.First(u => u.Id == id);

    public User GetUserByName(ApplicationDbContext dbContext, string name)
        => dbContext.Users.First(u => u.Name == name);

    public IQueryable<User> GetList(ApplicationDbContext dbContext)
        => dbContext.Users;
}
