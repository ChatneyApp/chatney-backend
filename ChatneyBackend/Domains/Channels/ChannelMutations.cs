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
        ChannelTypeDto channelTypeDto,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(ChannelPermissions.CreateChannel);

        var channelType = ChannelType.FromDto(channelTypeDto);
        channelType.Id = await repos.ChannelTypes.InsertOne(channelType);
        await webSocketConnector.SendNewChannelTypeAsync(WebsocketChannelTypePayload.FromChannelType(channelType));
        return channelType;
    }

    [Authorize]
    public async Task<ChannelType?> UpdateChannelType(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        ChannelType channelType,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannelType(channelType));
        permissions.Require(ChannelPermissions.EditChannel);

        var updated = await repos.ChannelTypes.UpdateOne(channelType);
        if (updated)
        {
            await webSocketConnector.SendUpdatedChannelTypeAsync(WebsocketChannelTypePayload.FromChannelType(channelType));
        }
        return updated ? channelType : null;
    }

    [Authorize]
    public async Task<bool> DeleteChannelType(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id,
        WebSocketConnector webSocketConnector)
    {
        var channelType = await repos.ChannelTypes.GetById(id);
        if (channelType == null)
        {
            return false;
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannelType(channelType));
        permissions.Require(ChannelPermissions.DeleteChannelType);

        var deleted = await repos.ChannelTypes.DeleteById(id);
        if (deleted)
        {
            await webSocketConnector.SendDeletedChannelTypeAsync(new WebsocketChannelTypeDeletedPayload { Id = id });
        }
        return deleted;
    }

    [Authorize]
    public async Task<Channel> AddChannel(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        ChannelDto channelDto,
        WebSocketConnector webSocketConnector)
    {
        var workspace = await repos.Workspaces.GetById(channelDto.WorkspaceId);
        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace!));
        permissions.Require(ChannelPermissions.CreateChannel);

        var channel = channelDto.ToModel();
        channel.Id = await repos.Channels.InsertOne(channel);
        await webSocketConnector.SendNewChannelAsync(WebsocketChannelPayload.FromChannel(channel));
        return channel;
    }

    [Authorize]
    public async Task<Channel?> UpdateChannel(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        Channel channel,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
        permissions.Require(ChannelPermissions.EditChannel);

        var updated = await repos.Channels.UpdateOne(channel);
        if (updated)
        {
            await webSocketConnector.SendUpdatedChannelAsync(WebsocketChannelPayload.FromChannel(channel));
        }
        return updated ? channel : null;
    }

    [Authorize]
    public async Task<bool> DeleteChannel(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id,
        WebSocketConnector webSocketConnector)
    {
        var channel = await repos.Channels.GetById(id);
        if (channel == null)
        {
            return false;
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
        permissions.Require(ChannelPermissions.DeleteChannel);

        var deleted = await repos.Channels.DeleteById(id);
        if (deleted)
        {
            await webSocketConnector.SendDeletedChannelAsync(new WebsocketChannelDeletedPayload
            {
                Id = id,
                WorkspaceId = channel.WorkspaceId,
            });
        }
        return deleted;
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

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace!));
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

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace));
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

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace));
        permissions.Require(ChannelPermissions.DeleteChannelGroup);

        return await repos.ChannelGroups.DeleteById(id);
    }
}
