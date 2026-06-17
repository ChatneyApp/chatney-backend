using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Channels;

public class ChannelQueries
{
    [Authorize]
    public async Task<Channel?> GetChannelById(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var channel = await repos.Channels.GetById(id);
        if (channel == null)
        {
            return null;
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
        permissions.Require(ChannelPermissions.ReadChannel);

        return channel;
    }

    [Authorize]
    public async Task<Channel?> GetChannelByName(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        string name)
    {
        var channel = await repos.Channels.GetOne(c => c.Name == name);
        if (channel == null)
        {
            return null;
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
        permissions.Require(ChannelPermissions.ReadChannel);

        return channel;
    }

    [Authorize]
    public async Task<List<Channel>> GetWorkspaceChannelList(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int workspaceId)
    {
        var workspace = await repos.Workspaces.GetById(workspaceId);
        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var user = await principal.GetRequiredUser(repos);

        var perms = await roleManager.GetUserPermissions(user, RoleScope.Global());

        var haveAccessToAnyChannel = perms.Can(ChannelPermissions.ReadMessage);
        if (haveAccessToAnyChannel)
        {
            return await repos.Channels.GetList(c => c.WorkspaceId == workspaceId);
        }

        var allUserChannels = await roleManager.GetPermittedChannels(repos, user.Id);
        var distinctChannelIds = allUserChannels
            .Select(c => c.Id)
            .Distinct()
            .ToList();

        var channels = await repos.Channels.GetList(
            c => distinctChannelIds.Contains(c.Id) &&
                 c.WorkspaceId == workspaceId
        );
        return channels;
    }

    [Authorize]
    public async Task<List<ChannelType>> GetChannelTypeList(AppRepos repos) => await repos.ChannelTypes.GetList();

    [Authorize]
    public async Task<List<ChannelGroup>> GetWorkspaceChannelGroupList(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int workspaceId)
    {
        var workspace = await repos.Workspaces.GetById(workspaceId);
        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace!));
        permissions.Require(ChannelPermissions.ReadChannel);

        return await repos.ChannelGroups.GetList(group => group.WorkspaceId == workspaceId);
    }
}
