using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Channels;

public class ChannelMutations
{
    [Authorize]
    public async Task<ChannelType> AddChannelType(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        ChannelTypeDto channelTypeDto)
    {
        var permissions = await roleManager.GetUserPermissions(repos, principal.GetUserGuid(), RoleScope.Global());
        permissions.Require(ChannelPermissions.CreateChannel);

        var channelType = ChannelType.FromDto(channelTypeDto);
        channelType.Id = await repos.ChannelTypes.InsertOne(channelType);
        return channelType;
    }

    [Authorize]
    public async Task<ChannelType?> UpdateChannelType(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        ChannelType channelType)
    {
        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromChannelType(channelType));
        permissions.Require(ChannelPermissions.EditChannel);

        var updated = await repos.ChannelTypes.UpdateOne(channelType);
        return updated ? channelType : null;
    }

    [Authorize]
    public async Task<bool> DeleteChannelType(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var channelType = await repos.ChannelTypes.GetById(id);
        if (channelType == null)
        {
            return false;
        }

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromChannelType(channelType));
        permissions.Require(ChannelPermissions.DeleteChannelType);

        return await repos.ChannelTypes.DeleteById(id);
    }

    [Authorize]
    public async Task<Channel> AddChannel(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        ChannelDto channelDto)
    {
        var workspace = await repos.Workspaces.GetById(channelDto.WorkspaceId);
        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromWorkspace(workspace!));
        permissions.Require(ChannelPermissions.CreateChannel);

        var channel = channelDto.ToModel();
        channel.Id = await repos.Channels.InsertOne(channel);
        return channel;
    }

    [Authorize]
    public async Task<Channel?> UpdateChannel(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        Channel channel)
    {
        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromChannel(channel));
        permissions.Require(ChannelPermissions.EditChannel);

        var updated = await repos.Channels.UpdateOne(channel);
        return updated ? channel : null;
    }

    [Authorize]
    public async Task<bool> DeleteChannel(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var channel = await repos.Channels.GetById(id);
        if (channel == null)
        {
            return false;
        }

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromChannel(channel));
        permissions.Require(ChannelPermissions.DeleteChannel);

        return await repos.Channels.DeleteById(id);
    }

    [Authorize]
    public async Task<ChannelGroup> AddChannelGroup(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        ChannelGroupDto channelGroupDto)
    {
        var workspace = await repos.Workspaces.GetById(channelGroupDto.WorkspaceId);
        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromWorkspace(workspace!));
        permissions.Require(ChannelPermissions.AddChannelGroup);

        var channelGroup = ChannelGroup.FromDto(channelGroupDto);
        channelGroup.Id = await repos.ChannelGroups.InsertOne(channelGroup);
        return channelGroup;
    }

    [Authorize]
    public async Task<ChannelGroup?> UpdateChannelGroup(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        ChannelGroup channelGroup)
    {
        var workspace = await repos.Workspaces.GetById(channelGroup.WorkspaceId);
        if (workspace == null)
        {
            return null;
        }

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromWorkspace(workspace));
        permissions.Require(ChannelPermissions.EditChannelGroup);

        var updated = await repos.ChannelGroups.UpdateOne(channelGroup);
        return updated ? channelGroup : null;
    }

    [Authorize]
    public async Task<bool> DeleteChannelGroup(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var channelGroup = await repos.ChannelGroups.GetById(id);
        if (channelGroup == null)
        {
            return false;
        }

        var workspace = await repos.Workspaces.GetById(channelGroup.WorkspaceId);
        if (workspace == null)
        {
            return false;
        }

        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.FromWorkspace(workspace));
        permissions.Require(ChannelPermissions.DeleteChannelGroup);

        return await repos.ChannelGroups.DeleteById(id);
    }
}
