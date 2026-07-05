using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Utils;

namespace ChatneyBackend.Domains.Roles;

public static class DefaultRoles
{
    public static string[] UserPermissions =>
    [
        ChannelPermissions.CreateMessage,
        ChannelPermissions.ReadChannel,
        ChannelPermissions.ReadMessage,
        ChannelPermissions.EditOwnMessage,
        ChannelPermissions.DeleteOwnMessage,
        WorkspacePermissions.ReadWorkspace,
        AttachmentPermissions.Upload,
        AttachmentPermissions.Read,
    ];

    public static string[] ModeratorPermissions =>
    [
        ..UserPermissions,
        UserPermissionNames.ReadUser,
        ChannelPermissions.EditMessage,
        ChannelPermissions.DeleteMessage,
    ];

    public static string[] AdminPermissions =>
    [
        ..PermissionsUtils.GetAllPermissions<SystemConfigPermissions>(),
        ..PermissionsUtils.GetAllPermissions<WorkspacePermissions>(),
        ..PermissionsUtils.GetAllPermissions<ChannelPermissions>(),
        ..PermissionsUtils.GetAllPermissions<UserPermissionNames>(),
        ..PermissionsUtils.GetAllPermissions<RolePermissions>(),
        ..PermissionsUtils.GetAllPermissions<AttachmentPermissions>(),
    ];

    public static Role[] CreateSeedRoles(DateTime timestamp) =>
    [
        new()
        {
            Name = DomainSettings.AdminRoleName,
            Permissions = AdminPermissions,
            IsProtected = true,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
        },
        new()
        {
            Name = DomainSettings.ModeratorRoleName,
            Permissions = ModeratorPermissions,
            IsProtected = true,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
        },
        new()
        {
            Name = DomainSettings.UserRoleName,
            Permissions = UserPermissions,
            IsProtected = true,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
        },
    ];
}
