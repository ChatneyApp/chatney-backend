using HotChocolate.Execution.Configuration;

namespace ChatneyBackend.Domains.Channels;

public class ChannelDomainSetup : IDomainSetup 
{
    public IRequestExecutorBuilder Setup(IRequestExecutorBuilder builder)
    {
        builder.AddTypeExtension<ChannelQueries>()
        .AddTypeExtension<ChannelMutations>();

        return builder;
    }
}