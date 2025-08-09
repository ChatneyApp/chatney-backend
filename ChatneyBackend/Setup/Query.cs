using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;

namespace ChatneyBackend.Setup;

public class Query
{
    public ChannelQueries Channels() => new();
    public ConfigQueries Configs() => new();
    public MessageQueries Messages() => new();
    public PermissionQueries Permissions() => new();
    public RoleQueries Roles() => new();
    public UserQueries Users() => new();
    public WorkspaceQueries Workspaces() => new();
}
