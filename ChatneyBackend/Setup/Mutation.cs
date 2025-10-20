using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Domains.InstallWizard;

namespace ChatneyBackend.Setup;

public class Mutation
{
    public ChannelMutations Channels() => new();
    public ConfigMutations Configs() => new();
    public MessageMutations Messages() => new();
    public RoleMutations Roles() => new();
    public UserMutations Users() => new UserMutations();
    public WorkspaceMutations Workspaces() => new();

    public InstallWizardMutations InstallWizard() => new();
}
