using HotChocolate.Execution.Configuration;

namespace ChatneyBackend.Domains.Users;

public class UserDomainSetup : IDomainSetup 
{
    public IRequestExecutorBuilder Setup(IRequestExecutorBuilder builder)
    {
        builder.AddTypeExtension<UserQueries>()
        .AddTypeExtension<UserMutations>();

        return builder;
    }
}