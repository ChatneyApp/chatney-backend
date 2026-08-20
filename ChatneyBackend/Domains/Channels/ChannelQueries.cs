using System.Security.Claims;
using ChatneyBackend.Domains.Permissions;
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
        IPermissionResolver resolver,
        int id)
    {
        var channel = await repos.Channels.GetById(id);
        if (channel == null)
        {
            return null;
        }

        var permissions = await resolver.ForChannel(channel);
        permissions.Require(Permission.ChannelReadChannel);

        return channel;
    }

    [Authorize]
    public async Task<Channel?> GetChannelByName(
        AppRepos repos,
        IPermissionResolver resolver,
        string name)
    {
        var channel = await repos.Channels.GetOne(c => c.Name == name);
        if (channel == null)
        {
            return null;
        }

        var permissions = await resolver.ForChannel(channel);
        permissions.Require(Permission.ChannelReadChannel);

        return channel;
    }

    [Authorize]
    public async Task<List<Channel>> GetWorkspaceChannelList(
        AppRepos repos,
        IPermissionResolver resolver,
        int workspaceId)
    {
        var workspace = await repos.Workspaces.GetById(workspaceId);
        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        return await resolver.VisibleChannels(workspaceId, Permission.ChannelReadChannel);
    }

    [Authorize]
    public async Task<List<DirectMessage>> GetDirectMessageList(
        AppRepos repos,
        ClaimsPrincipal principal)
    {
        var actorId = principal.GetUserGuid();
        var memberships = await repos.ChannelMembers.GetList(member => member.UserId == actorId);
        if (memberships.Count == 0)
        {
            return [];
        }

        var channelIds = memberships.Select(member => member.ChannelId).ToList();
        var channels = await repos.Channels.GetList(channel =>
            channelIds.Contains(channel.Id) && channel.IsDm);

        var result = new List<DirectMessage>(channels.Count);
        foreach (var channel in channels)
        {
            result.Add(await ChannelMembership.ToDirectMessage(repos, channel, actorId));
        }

        return result;
    }

    [Authorize]
    public async Task<List<ChannelType>> GetChannelTypeList(AppRepos repos) => await repos.ChannelTypes.GetList();

    [Authorize]
    public async Task<List<ChannelGroup>> GetWorkspaceChannelGroupList(
        AppRepos repos,
        IPermissionResolver resolver,
        int workspaceId)
    {
        var workspace = await repos.Workspaces.GetById(workspaceId);
        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var permissions = await resolver.ForWorkspace(workspace!);
        permissions.Require(Permission.ChannelReadChannel);

        return await repos.ChannelGroups.GetList(group => group.WorkspaceId == workspaceId);
    }
}
