using NpgsqlTypes;

namespace ChatneyBackend.Domains.Permissions;

public enum Permission
{
    [PgName("attachment.delete")]
    AttachmentDelete,

    [PgName("attachment.read")]
    AttachmentRead,

    [PgName("attachment.upload")]
    AttachmentUpload,

    [PgName("channel.addChannelGroup")]
    ChannelAddChannelGroup,

    [PgName("channel.createChannel")]
    ChannelCreateChannel,

    [PgName("channel.createMessage")]
    ChannelCreateMessage,

    [PgName("channel.deleteChannel")]
    ChannelDeleteChannel,

    [PgName("channel.deleteChannelGroup")]
    ChannelDeleteChannelGroup,

    [PgName("channel.deleteChannelType")]
    ChannelDeleteChannelType,

    [PgName("channel.deleteMessage")]
    ChannelDeleteMessage,

    [PgName("channel.deleteOwnMessage")]
    ChannelDeleteOwnMessage,

    [PgName("channel.editChannel")]
    ChannelEditChannel,

    [PgName("channel.editChannelGroup")]
    ChannelEditChannelGroup,

    [PgName("channel.editMessage")]
    ChannelEditMessage,

    [PgName("channel.editOwnMessage")]
    ChannelEditOwnMessage,

    [PgName("channel.readChannel")]
    ChannelReadChannel,

    [PgName("channel.readMessage")]
    ChannelReadMessage,

    [PgName("config.readValue")]
    ConfigReadValue,

    [PgName("config.updateValue")]
    ConfigUpdateValue,

    [PgName("role.createRole")]
    RoleCreateRole,

    [PgName("role.deleteRole")]
    RoleDeleteRole,

    [PgName("role.editRole")]
    RoleEditRole,

    [PgName("user.createUser")]
    UserCreateUser,

    [PgName("user.deleteUser")]
    UserDeleteUser,

    [PgName("user.editUser")]
    UserEditUser,

    [PgName("user.readUser")]
    UserReadUser,

    [PgName("workspace.createWorkspace")]
    WorkspaceCreateWorkspace,

    [PgName("workspace.deleteWorkspace")]
    WorkspaceDeleteWorkspace,

    [PgName("workspace.readWorkspace")]
    WorkspaceReadWorkspace,

    [PgName("workspace.updateWorkspace")]
    WorkspaceUpdateWorkspace
}
