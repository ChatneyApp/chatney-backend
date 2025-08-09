using HotChocolate.Execution.Configuration;

namespace ChatneyBackend.Domains.Configs;

public class ConfigDomainSetup : IDomainSetup 
{
    public IRequestExecutorBuilder Setup(IRequestExecutorBuilder builder)
    {
        builder.AddTypeExtension<ConfigQueries>()
        .AddTypeExtension<ConfigMutations>();

        return builder;
    }
} 