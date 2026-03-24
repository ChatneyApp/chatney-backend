using ChatneyBackend.Infra;
using ChatneyBackend.Domains.Users;

namespace ChatneyBackend.Domains.Roles;

public sealed record RoleScope(string? WorkspaceId, string? ChannelId, string? ChannelTypeId);

public class RoleManager
{
    private readonly PgRepo<Role, int> _roles;

    public RoleManager(PgRepo<Role, int> roles)
    {
        _roles = roles;
    }

    public async Task<Role?> GetRelevantRole(
        User user,
        RoleScope roleScope
    )
    {
        if (roleScope.ChannelId != null && user.Roles.Channel.ContainsKey(roleScope.ChannelId))
        {
            var channelRole = user.Roles.Channel[roleScope.ChannelId];
            return await _roles.GetById(channelRole.RoleId);
        }

        if (roleScope.ChannelTypeId != null && user.Roles.ChannelTypes.ContainsKey(roleScope.ChannelTypeId))
        {
            var channelTypeRole = user.Roles.ChannelTypes[roleScope.ChannelTypeId];
            return await _roles.GetById(channelTypeRole.RoleId);
        }

        if (roleScope.WorkspaceId != null && user.Roles.Workspace.ContainsKey(roleScope.WorkspaceId))
        {
            var workspaceRole = user.Roles.Workspace[roleScope.WorkspaceId];
            return await _roles.GetById(workspaceRole.RoleId);
        }

        return await _roles.GetById(user.Roles.Global);
    }
}
