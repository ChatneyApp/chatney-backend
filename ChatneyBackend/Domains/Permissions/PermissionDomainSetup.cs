using HotChocolate.Execution.Configuration;

namespace ChatneyBackend.Domains.Permissions;

public class PermissionDomainSetup : IDomainSetup 
{
    public IRequestExecutorBuilder Setup(IRequestExecutorBuilder builder)
    {
        builder.AddTypeExtension<PermissionQueries>()
        .AddTypeExtension<PermissionMutations>();

        return builder;
    }
} 