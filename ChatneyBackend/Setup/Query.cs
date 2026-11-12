using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.DraftMessages;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;

namespace ChatneyBackend.Setup;

public class Query
{
    /// <summary>File attachments: metadata lookup.</summary>
    public AttachmentQueries Attachments() => new();
    /// <summary>Channels, channel types, channel groups and direct messages.</summary>
    public ChannelQueries Channels() => new();
    /// <summary>System-wide configuration values.</summary>
    public ConfigQueries Configs() => new();
    /// <summary>Channel messages, threads and reactions.</summary>
    public MessageQueries Messages() => new();
    /// <summary>The current user's unsent message drafts.</summary>
    public DraftMessageQueries DraftMessages() => new();
    /// <summary>Catalog of all permissions, grouped for the admin UI.</summary>
    public PermissionQueries Permissions() => new();
    /// <summary>Raw role/user ACL rows (admin UI).</summary>
    public AclQueries Acls() => new();
    /// <summary>Roles.</summary>
    public RoleQueries Roles() => new();
    /// <summary>Users, the current user's profile and resolved permissions.</summary>
    public UserQueries Users() => new();
    /// <summary>Workspaces.</summary>
    public WorkspaceQueries Workspaces() => new();
}
