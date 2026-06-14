using ChatneyBackend.Infra;
using ChatneyBackend.Domains.Users;

namespace ChatneyBackend.Domains.Roles;

public sealed record RoleScope(int? WorkspaceId, int? ChannelId, int? ChannelTypeId);

public class RoleManager
{
    private readonly PgRepo<Role, int> _roles;
    private readonly PgRepo<UserRole, UserRoleKey> _userRoles;

    public RoleManager(PgRepo<Role, int> roles, PgRepo<UserRole, UserRoleKey> userRoles)
    {
        _roles = roles;
        _userRoles = userRoles;
    }

    public async Task<UserPermissions> GetUserPermissions(
        User user,
        RoleScope roleScope
    )
    {
        var userRoles = await _userRoles.GetList(r => r.UserId == user.Id);

        if (roleScope.ChannelId != null)
        {
            var role = userRoles.Find(role => role.ChannelId == roleScope.ChannelId);
            if (role != null)
            {
                return await FromRoleId(role.RoleId);
            }
        }

        if (roleScope.ChannelTypeId != null)
        {
            var role = userRoles.Find(role => role.ChannelTypeId == roleScope.ChannelTypeId);
            if (role != null)
            {
                return await FromRoleId(role.RoleId);
            }
        }

        if (roleScope.WorkspaceId != null)
        {
            var role = userRoles.Find(role => role.WorkspaceId == roleScope.WorkspaceId);
            if (role != null)
            {
                return await FromRoleId(role.RoleId);
            }
        }

        return await FromRoleId(user.RoleId);
    }

    private async Task<UserPermissions> FromRoleId(int roleId)
    {
        var role = await _roles.GetById(roleId);
        return new UserPermissions(role?.Permissions ?? []);
    }
}
