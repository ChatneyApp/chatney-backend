using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Utils;

namespace ChatneyBackend.Domains.Permissions;

public class PermissionQueries
{
    public string[] GetList()
    {
        return new[] {
            PermissionsUtils.GetAllPermissions<ChannelPermissions>(),
            PermissionsUtils.GetAllPermissions<SystemConfigPermissions>(),
            PermissionsUtils.GetAllPermissions<RolePermissions>(),
            PermissionsUtils.GetAllPermissions<UserPermissions>(),
            PermissionsUtils.GetAllPermissions<WorkspacePermissions>()
        }.SelectMany(inner => inner).ToArray();
    }
}
