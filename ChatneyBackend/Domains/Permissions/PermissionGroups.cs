namespace ChatneyBackend.Domains.Permissions;

public static class PermissionGroups
{
    public static readonly PermissionGroup[] All;

    static PermissionGroups()
    {
        All =
        [
            new PermissionGroup("System config permissions",
            [
                Permission.ConfigReadValue,
                Permission.ConfigUpdateValue
            ]),
            new PermissionGroup("Workspace permissions",
            [
                Permission.WorkspaceCreateWorkspace,
                Permission.WorkspaceDeleteWorkspace,
                Permission.WorkspaceReadWorkspace,
                Permission.WorkspaceUpdateWorkspace
            ]),
            new PermissionGroup("Channel permissions",
            [
                Permission.ChannelAddChannelGroup,
                Permission.ChannelCreateChannel,
                Permission.ChannelCreateMessage,
                Permission.ChannelDeleteChannel,
                Permission.ChannelDeleteChannelGroup,
                Permission.ChannelDeleteChannelType,
                Permission.ChannelDeleteMessage,
                Permission.ChannelDeleteOwnMessage,
                Permission.ChannelEditChannel,
                Permission.ChannelEditChannelGroup,
                Permission.ChannelEditMessage,
                Permission.ChannelEditOwnMessage,
                Permission.ChannelReadChannel,
                Permission.ChannelReadMessage
            ]),
            new PermissionGroup("User permissions",
            [
                Permission.UserCreateUser,
                Permission.UserDeleteUser,
                Permission.UserEditUser,
                Permission.UserReadUser
            ]),
            new PermissionGroup("Role permissions",
            [
                Permission.RoleCreateRole,
                Permission.RoleDeleteRole,
                Permission.RoleEditRole
            ]),
            new PermissionGroup("Attachment permissions",
            [
                Permission.AttachmentDelete,
                Permission.AttachmentRead,
                Permission.AttachmentUpload
            ])
        ];

        var allValues = Enum.GetValues<Permission>();
        var grouped = All.SelectMany(g => g.List).ToArray();
        var groupedSet = grouped.ToHashSet();

        if (groupedSet.Count != grouped.Length)
        {
            throw new InvalidOperationException(
                "PermissionGroups drift detected: at least one Permission value appears in more than one group.");
        }

        var missing = allValues.Where(p => !groupedSet.Contains(p)).ToArray();

        if (missing.Length > 0)
        {
            throw new InvalidOperationException(
                $"PermissionGroups drift detected: the following Permission values are not assigned to any group: {string.Join(", ", missing)}.");
        }

        var unknown = groupedSet.Where(p => !allValues.Contains(p)).ToArray();

        if (unknown.Length > 0)
        {
            throw new InvalidOperationException(
                $"PermissionGroups drift detected: the following grouped values are not defined in the Permission enum: {string.Join(", ", unknown)}.");
        }
    }
}
