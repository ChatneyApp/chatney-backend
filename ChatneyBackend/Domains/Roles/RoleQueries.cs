using System.Security.Claims;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Roles;

public class RoleQueries
{
    public record ScopedPermissions
    {
        public List<string> GlobalPermissions { get; init; }
        public Dictionary<int, List<string>> WorkspacePermissions { get; init; }
        public Dictionary<int, List<string>> ChannelTypePermissions { get; init; }
        public Dictionary<int, List<string>> ChannelPermissions { get; init; }
    }

    [Authorize]
    public async Task<Role?> GetRoleById(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.ReadRole);

        return await repos.Roles.GetById(id);
    }

    [Authorize]
    public async Task<Role?> GetRoleByName(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        string name)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.ReadRole);

        return await repos.Roles.GetOne(r => r.Name == name);
    }

    [Authorize]
    public async Task<List<Role>> GetList(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(RolePermissions.ReadRole);

        return await repos.Roles.GetList();
    }

    [Authorize]
    public async Task<ScopedPermissions> GetMyRoles(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal)
    {
        var user = await principal.GetRequiredUser(repos);
        var userRoles = await repos.UserRoles.GetList(r => r.UserId == user.Id);
        var roleIds = userRoles.Select(r => r.RoleId).Concat([user.RoleId]).Distinct().ToList();
        var roles = await repos.Roles.GetList(r => roleIds.Contains(r.Id));
        var userGlobalRole = roles.Find(r => r.Id == user.RoleId);

        List<string> userGlobalPermissions = new List<string>(userGlobalRole?.Permissions ?? Array.Empty<string>());

        Dictionary<int, List<string>> workspacePermissions = userRoles
            .Select(userRole => new
            {
                UserRole = userRole,
                Role = roles.Find(r => r.Id == userRole.RoleId)
            })
            .Where(item => item.UserRole.WorkspaceId != null && item.Role != null)
            .ToDictionary(
                item => item.UserRole.WorkspaceId!.Value,
                item => userGlobalPermissions
                    .Concat(item.Role!.Permissions)
                    .Concat(item.UserRole.Allowlist)
                    .Except(item.UserRole.Denylist)
                    .Distinct()
                    .ToList());

        Dictionary<int, List<string>> channelTypePermissions = userRoles
            .Select(userRole => new
            {
                UserRole = userRole,
                Role = roles.Find(r => r.Id == userRole.RoleId)
            })
            .Where(item => item.UserRole.ChannelTypeId != null && item.Role != null)
            .ToDictionary(
                item => item.UserRole.ChannelTypeId!.Value,
                item => userGlobalPermissions
                    .Concat(item.Role!.Permissions)
                    .Concat(item.UserRole.Allowlist)
                    .Except(item.UserRole.Denylist)
                    .Distinct()
                    .ToList());

        var channelIds = userRoles
            .Where(userRole => userRole.ChannelId != null)
            .Select(userRole => userRole.ChannelId!.Value)
            .Distinct()
            .ToList();
        var channels = await repos.Channels.GetList(channel => channelIds.Contains(channel.Id));

        Dictionary<int, List<string>> channelPermissions = userRoles
            .Select(userRole => new
            {
                UserRole = userRole,
                Role = roles.Find(r => r.Id == userRole.RoleId),
                Channel = userRole.ChannelId == null
                    ? null
                    : channels.Find(channel => channel.Id == userRole.ChannelId)
            })
            .Where(item => item.UserRole.ChannelId != null && item.Role != null)
            .ToDictionary(
                item => item.UserRole.ChannelId!.Value,
                item =>
                {
                    var permissions = new List<string>(userGlobalPermissions);

                    if (item.Channel != null &&
                        workspacePermissions.TryGetValue(item.Channel.WorkspaceId, out var workspacePermissionList))
                    {
                        permissions.AddRange(workspacePermissionList);
                    }

                    if (item.Channel != null &&
                        channelTypePermissions.TryGetValue(item.Channel.ChannelTypeId, out var channelTypePermissionList))
                    {
                        permissions.AddRange(channelTypePermissionList);
                    }

                    permissions.AddRange(item.Role!.Permissions);
                    permissions.AddRange(item.UserRole.Allowlist);

                    return permissions
                        .Except(item.UserRole.Denylist)
                        .Distinct()
                        .ToList();
                });

        ScopedPermissions result = new ScopedPermissions
        {
            GlobalPermissions = userGlobalPermissions,
            WorkspacePermissions = workspacePermissions,
            ChannelTypePermissions = channelTypePermissions,
            ChannelPermissions = channelPermissions,
        };

        return result;
    }
}
