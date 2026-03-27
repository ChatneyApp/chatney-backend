using ChatneyBackend.Infra;
using ChatneyBackend.Domains.Users;

namespace ChatneyBackend.Domains.Roles;

public sealed record RoleScope(int? WorkspaceId, string? ChannelId, string? ChannelTypeId);

public class RoleManager
{
    private readonly PgRepo<Role, int> _roles;

    public RoleManager(PgRepo<Role, int> roles)
    {
        _roles = roles;
    }

    public async Task<Role?> GetRelevantRole(
        User user,
        List<UserRole> userRoles,
        RoleScope roleScope
    )
    {
        if (roleScope.ChannelId != null)
        {
            var role = userRoles.Find(role => role.Type == "channel" && role.ItemId == roleScope.ChannelId);
            if (role != null)
            {
                return await _roles.GetById(role.RoleId);
            }
        }

        if (roleScope.ChannelTypeId != null)
        {
            var role = userRoles.Find(role => role.Type == "channel_type" && role.ItemId == roleScope.ChannelTypeId);
            if (role != null)
            {
                return await _roles.GetById(role.RoleId);
            }
        }

        if (roleScope.WorkspaceId != null)
        {
            var role = userRoles.Find(role => role.Type == "workspace" && int.Parse(role.ItemId) == roleScope.WorkspaceId);
            if (role != null)
            {
                return await _roles.GetById(role.RoleId);
            }
        }

        return await _roles.GetById(user.RoleId);
    }
}
