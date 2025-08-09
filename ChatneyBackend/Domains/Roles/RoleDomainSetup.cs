using HotChocolate.Execution.Configuration;

namespace ChatneyBackend.Domains.Roles;

public class RoleDomainSetup : IDomainSetup 
{
    public IRequestExecutorBuilder Setup(IRequestExecutorBuilder builder)
    {
        builder.AddTypeExtension<RoleQueries>()
        .AddTypeExtension<RoleMutations>();

        return builder;
    }
} 