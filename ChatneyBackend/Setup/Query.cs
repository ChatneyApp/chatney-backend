using ChatneyBackend.Domains.Users;

namespace ChatneyBackend.Setup;

public class Query
{
    public UserQueries Users() => new UserQueries();
}
