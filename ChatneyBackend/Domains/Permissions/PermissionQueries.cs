using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Utils;

namespace ChatneyBackend.Domains.Permissions;

public record PermissionGroup(string Label, string[] List);

public class PermissionQueries
{
    public PermissionGroup[] GetList()
    {
        return
        [
            new PermissionGroup("System config permissions", PermissionsUtils.GetAllPermissions<ChannelPermissions>()),
            new PermissionGroup("Workspace permissions", PermissionsUtils.GetAllPermissions<SystemConfigPermissions>()),
            new PermissionGroup("Channel permissions", PermissionsUtils.GetAllPermissions<RolePermissions>()),
            new PermissionGroup("Message permissions", PermissionsUtils.GetAllPermissions<MessagePermissions>()),
            new PermissionGroup("User permissions", PermissionsUtils.GetAllPermissions<UserPermissions>()),
            new PermissionGroup("Role permissions", PermissionsUtils.GetAllPermissions<WorkspacePermissions>())
        ];
    }
}
