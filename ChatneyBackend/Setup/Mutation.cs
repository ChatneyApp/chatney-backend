using ChatneyBackend.Domains.Users;

namespace ChatneyBackend.Setup;

public class Mutation
{
    public UserMutations Users() => new UserMutations();
}
