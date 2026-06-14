using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
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

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromChannel(channel));
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

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromChannel(channel));
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

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromWorkspace(workspace!));
        permissions.Require(ChannelPermissions.ReadChannel);

        return await repos.Channels.GetList(channel => channel.WorkspaceId == workspaceId);
    }

    [Authorize]
    public async Task<List<ChannelType>> GetChannelTypeList(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal)
    {
        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.Global());
        permissions.Require(ChannelPermissions.ReadChannel);

        return await repos.ChannelTypes.GetList();
    }

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

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromWorkspace(workspace!));
        permissions.Require(ChannelPermissions.ReadChannel);

        return await repos.ChannelGroups.GetList(group => group.WorkspaceId == workspaceId);
    }
}
