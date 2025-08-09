using HotChocolate.Execution.Configuration;

namespace ChatneyBackend.Domains.Messages;

public class MessageDomainSetup : IDomainSetup 
{
    public IRequestExecutorBuilder Setup(IRequestExecutorBuilder builder)
    {
        builder.AddTypeExtension<MessageQueries>()
        .AddTypeExtension<MessageMutations>();

        return builder;
    }
} 