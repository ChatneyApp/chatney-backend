using NpgsqlTypes;

namespace ChatneyBackend.Domains.Permissions;

/// <summary>An action a subject (role or user) can be granted on a secure object via an ACL.</summary>
public enum Permission
{
    /// <summary>Delete attachments. Checked globally.</summary>
    [PgName("attachment.delete")]
    AttachmentDelete,

    /// <summary>Read attachment metadata. Checked globally.</summary>
    [PgName("attachment.read")]
    AttachmentRead,

    /// <summary>Upload attachments. Checked globally.</summary>
    [PgName("attachment.upload")]
    AttachmentUpload,

    /// <summary>Create channel groups in a workspace. Checked on the workspace.</summary>
    [PgName("channel.addChannelGroup")]
    ChannelAddChannelGroup,

    /// <summary>Create channels in a workspace (checked on the workspace); granted globally, also allows creating channel types.</summary>
    [PgName("channel.createChannel")]
    ChannelCreateChannel,

    /// <summary>Post messages in a channel. Checked on the channel.</summary>
    [PgName("channel.createMessage")]
    ChannelCreateMessage,

    /// <summary>Delete a channel. Checked on the channel.</summary>
    [PgName("channel.deleteChannel")]
    ChannelDeleteChannel,

    /// <summary>Delete channel groups in a workspace. Checked on the workspace.</summary>
    [PgName("channel.deleteChannelGroup")]
    ChannelDeleteChannelGroup,

    /// <summary>Delete a channel type. Checked on the channel type.</summary>
    [PgName("channel.deleteChannelType")]
    ChannelDeleteChannelType,

    /// <summary>Delete anyone's messages in a channel. Checked on the channel.</summary>
    [PgName("channel.deleteMessage")]
    ChannelDeleteMessage,

    /// <summary>Delete your own messages in a channel. Checked on the channel.</summary>
    [PgName("channel.deleteOwnMessage")]
    ChannelDeleteOwnMessage,

    /// <summary>Edit a channel (checked on the channel) or a channel type (checked on the channel type).</summary>
    [PgName("channel.editChannel")]
    ChannelEditChannel,

    /// <summary>Edit channel groups in a workspace. Checked on the workspace.</summary>
    [PgName("channel.editChannelGroup")]
    ChannelEditChannelGroup,

    /// <summary>Edit anyone's messages in a channel. Checked on the channel.</summary>
    [PgName("channel.editMessage")]
    ChannelEditMessage,

    /// <summary>Edit your own messages in a channel. Checked on the channel.</summary>
    [PgName("channel.editOwnMessage")]
    ChannelEditOwnMessage,

    /// <summary>See a channel; channels without it are hidden. Checked on the channel.</summary>
    [PgName("channel.readChannel")]
    ChannelReadChannel,

    /// <summary>Read messages in a channel. Checked on the channel.</summary>
    [PgName("channel.readMessage")]
    ChannelReadMessage,

    /// <summary>Read system configuration values. Checked globally.</summary>
    [PgName("config.readValue")]
    ConfigReadValue,

    /// <summary>Update system configuration values. Checked globally.</summary>
    [PgName("config.updateValue")]
    ConfigUpdateValue,

    /// <summary>Create roles. Checked globally.</summary>
    [PgName("role.createRole")]
    RoleCreateRole,

    /// <summary>Delete roles. Checked globally.</summary>
    [PgName("role.deleteRole")]
    RoleDeleteRole,

    /// <summary>Edit roles and role ACLs. Checked globally.</summary>
    [PgName("role.editRole")]
    RoleEditRole,

    /// <summary>Create users as an admin. Checked globally.</summary>
    [PgName("user.createUser")]
    UserCreateUser,

    /// <summary>Delete users. Checked globally.</summary>
    [PgName("user.deleteUser")]
    UserDeleteUser,

    /// <summary>Edit other users, their role assignments and user ACLs. Checked globally.</summary>
    [PgName("user.editUser")]
    UserEditUser,

    /// <summary>List and look up users. Checked globally.</summary>
    [PgName("user.readUser")]
    UserReadUser,

    /// <summary>Create workspaces. Checked globally.</summary>
    [PgName("workspace.createWorkspace")]
    WorkspaceCreateWorkspace,

    /// <summary>Delete a workspace. Checked on the workspace.</summary>
    [PgName("workspace.deleteWorkspace")]
    WorkspaceDeleteWorkspace,

    /// <summary>See a workspace. Checked on the workspace.</summary>
    [PgName("workspace.readWorkspace")]
    WorkspaceReadWorkspace,

    /// <summary>Edit a workspace. Checked on the workspace.</summary>
    [PgName("workspace.updateWorkspace")]
    WorkspaceUpdateWorkspace
}
