using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.DraftMessages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Domains.InstallWizard;

namespace ChatneyBackend.Setup;

public class Mutation
{
    /// <summary>Channels, channel types, channel groups and direct messages.</summary>
    public ChannelMutations Channels() => new();
    /// <summary>Update system-wide configuration values.</summary>
    public ConfigMutations Configs() => new();
    /// <summary>Post, edit and delete messages; manage reactions.</summary>
    public MessageMutations Messages() => new();
    /// <summary>Save and clear the current user's message drafts.</summary>
    public DraftMessageMutations DraftMessages() => new();
    /// <summary>Upload and delete file attachments.</summary>
    public AttachmentMutations Attachments() => new();
    /// <summary>Grant or revoke permissions for roles and users on secure objects.</summary>
    public AclMutations Acls() => new();
    /// <summary>Create, edit and delete roles.</summary>
    public RoleMutations Roles() => new();
    /// <summary>Registration, login and user management.</summary>
    public UserMutations Users() => new();
    /// <summary>Assign roles to users and remove them.</summary>
    public UserRoleMutations UserRoles() => new();
    /// <summary>Create, edit and delete workspaces.</summary>
    public WorkspaceMutations Workspaces() => new();

    /// <summary>System install and uninstall. No auth required.</summary>
    public InstallWizardMutations InstallWizard() => new();
}
