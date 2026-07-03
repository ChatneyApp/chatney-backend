using ChatneyBackend.Infra;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;

namespace ChatneyBackend.Domains.Roles;

public sealed record RoleScope(int? WorkspaceId, int? ChannelId, int? ChannelTypeId)
{
    public static RoleScope FromChannel(Channel channel) =>
        new(channel.WorkspaceId, channel.Id, channel.ChannelTypeId);

    public static RoleScope FromChannelType(ChannelType channelType) =>
        new(null, null, channelType.Id);

    public static RoleScope FromWorkspace(Workspace workspace) =>
        new(workspace.Id, null, null);

    public static RoleScope Global() =>
        new(null, null, null);
}

public class RoleManager
{
    private readonly IPgRepo<Role, int> _roles;
    private readonly IPgRepo<UserRole, UserRoleKey> _userRoles;

    public RoleManager(IPgRepo<Role, int> roles, IPgRepo<UserRole, UserRoleKey> userRoles)
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
                return await FromUserRole(role);
            }
        }

        if (roleScope.ChannelTypeId != null)
        {
            var role = userRoles.Find(role => role.ChannelTypeId == roleScope.ChannelTypeId);
            if (role != null)
            {
                return await FromUserRole(role);
            }
        }

        if (roleScope.WorkspaceId != null)
        {
            var role = userRoles.Find(role => role.WorkspaceId == roleScope.WorkspaceId);
            if (role != null)
            {
                return await FromUserRole(role);
            }
        }

        return await FromRoleId(user.RoleId);
    }

    public async Task<List<Channel>> GetPermittedChannels(AppRepos repos, Guid userId)
    {
        var userRoles = await repos.UserRoles.GetList(r => r.UserId == userId);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await repos.Roles.GetList(r => roleIds.Any(rId => r.Id == rId));
        var channels = await repos.Channels.GetList();

        var channelsByChannelTypesForUser = channels
            .FindAll(c =>
            {
                var userRole = userRoles.Find(r => r.ChannelTypeId == c.ChannelTypeId);
                if (userRole == null)
                {
                    return false;
                }

                var role = roles.Find(r => r.Id == userRole.RoleId);
                if (role == null)
                {
                    return false;
                }

                return role.Permissions.Contains(ChannelPermissions.ReadMessage);
            });

        var channelsByworkspacesForUser = channels
            .FindAll(c =>
            {
                var userRole = userRoles.Find(r => r.WorkspaceId == c.WorkspaceId);
                if (userRole == null)
                {
                    return false;
                }

                var role = roles.Find(r => r.Id == userRole.RoleId);
                if (role == null)
                {
                    return false;
                }

                return role.Permissions.Contains(ChannelPermissions.ReadMessage);
            });

        var channelsForUser = channels.FindAll(c =>
        {
            var userRole = userRoles.Find(r => r.ChannelId == c.Id);
            if (userRole == null)
            {
                return false;
            }

            var role = roles.Find(r => r.Id == userRole.RoleId);
            if (role == null)
            {
                return false;
            }

            return role.Permissions.Contains(ChannelPermissions.ReadMessage);
        });


        // Combine all relevant channels for the user and take distinct workspace ids
        return channelsByChannelTypesForUser
            .Concat(channelsByworkspacesForUser)
            .Concat(channelsForUser)
            .ToList();
    }

    private async Task<UserPermissions> FromUserRole(UserRole userRole)
    {
        var role = await _roles.GetById(userRole.RoleId);
        return new UserPermissions(role?.Permissions ?? [], userRole.Allowlist, userRole.Denylist);
    }

    private async Task<UserPermissions> FromRoleId(int roleId)
    {
        var role = await _roles.GetById(roleId);
        return new UserPermissions(role?.Permissions ?? [], [], []);
    }
}
