using HotChocolate.Execution.Configuration;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceDomainSetup : IDomainSetup 
{
    public IRequestExecutorBuilder Setup(IRequestExecutorBuilder builder)
    {
        builder.AddTypeExtension<WorkspaceQueries>()
        .AddTypeExtension<WorkspaceMutations>();

        return builder;
    }
} 