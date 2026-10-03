using System.Security.Claims;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Channels;

public class ChannelMutations
{
    /// <summary>Creates a channel type. Requires ChannelCreateChannel globally.</summary>
    [Authorize]
    public async Task<ChannelType> AddChannelType(
        AppRepos repos,
        IPermissionResolver resolver,
        ChannelTypeDto channelTypeDto,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.ChannelCreateChannel);

        var channelType = ChannelType.FromDto(channelTypeDto);
        channelType.SecObjId = await SecureObjectHelper.Create(
            repos,
            new SecureObjectDescription { Kind = "channelType", Name = channelType.Name });
        channelType.Id = await repos.ChannelTypes.InsertOne(channelType);
        await webSocketConnector.SendNewChannelTypeAsync(WebsocketChannelTypePayload.FromChannelType(channelType));
        resolver.Invalidate();
        return channelType;
    }

    /// <summary>Updates a channel type, matched by id. Requires ChannelEditChannel on it.</summary>
    /// <returns>The saved channel type, or null if it doesn't exist.</returns>
    [Authorize]
    public async Task<ChannelType?> UpdateChannelType(
        AppRepos repos,
        IPermissionResolver resolver,
        ChannelType channelType,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.ForChannelType(channelType);
        permissions.Require(Permission.ChannelEditChannel);

        var updated = await repos.ChannelTypes.UpdateOne(channelType);
        if (updated)
        {
            await webSocketConnector.SendUpdatedChannelTypeAsync(WebsocketChannelTypePayload.FromChannelType(channelType));
        }
        return updated ? channelType : null;
    }

    /// <summary>Deletes a channel type. Requires ChannelDeleteChannelType on it.</summary>
    /// <returns>True if it existed and was deleted.</returns>
    [Authorize]
    public async Task<bool> DeleteChannelType(
        AppRepos repos,
        IPermissionResolver resolver,
        int id,
        WebSocketConnector webSocketConnector)
    {
        var channelType = await repos.ChannelTypes.GetById(id);
        if (channelType == null)
        {
            return false;
        }

        var permissions = await resolver.ForChannelType(channelType);
        permissions.Require(Permission.ChannelDeleteChannelType);

        var deleted = await repos.ChannelTypes.DeleteById(id);
        if (deleted)
        {
            await webSocketConnector.SendDeletedChannelTypeAsync(new WebsocketChannelTypeDeletedPayload { Id = id });
        }
        return deleted;
    }

    /// <summary>
    /// Creates a channel in a workspace. Requires ChannelCreateChannel on the workspace.
    /// NOT_FOUND if the workspace doesn't exist, FORBIDDEN_ACTION if the channel type doesn't exist.
    /// </summary>
    [Authorize]
    public async Task<Channel> AddChannel(
        AppRepos repos,
        IPermissionResolver resolver,
        ChannelDto channelDto,
        WebSocketConnector webSocketConnector)
    {
        var workspace = await repos.Workspaces.GetById(channelDto.WorkspaceId);
        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var channelType = await repos.ChannelTypes.GetById(channelDto.ChannelTypeId);
        if (channelType == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        var permissions = await resolver.ForWorkspace(workspace!);
        permissions.Require(Permission.ChannelCreateChannel);

        var channel = channelDto.ToModel();
        channel.SecObjId = await SecureObjectHelper.Create(
            repos,
            new SecureObjectDescription { Kind = "channel", Name = channel.Name });
        channel.Id = await repos.Channels.InsertOne(channel);
        await webSocketConnector.SendNewChannelAsync(WebsocketChannelPayload.FromChannel(channel));
        resolver.Invalidate();
        return channel;
    }

    /// <summary>Updates a channel, matched by id. Requires ChannelEditChannel on it.</summary>
    /// <returns>The saved channel, or null if it doesn't exist.</returns>
    [Authorize]
    public async Task<Channel?> UpdateChannel(
        AppRepos repos,
        IPermissionResolver resolver,
        Channel channel,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.ForChannel(channel);
        permissions.Require(Permission.ChannelEditChannel);

        var updated = await repos.Channels.UpdateOne(channel);
        if (updated)
        {
            var memberUserIds = await ChannelMembership.FanoutUserIds(repos, channel);
            await webSocketConnector.SendUpdatedChannelAsync(
                WebsocketChannelPayload.FromChannel(channel, memberUserIds));
        }
        return updated ? channel : null;
    }

    /// <summary>Deletes a channel. Requires ChannelDeleteChannel on it.</summary>
    /// <returns>True if it existed and was deleted.</returns>
    [Authorize]
    public async Task<bool> DeleteChannel(
        AppRepos repos,
        IPermissionResolver resolver,
        int id,
        WebSocketConnector webSocketConnector)
    {
        var channel = await repos.Channels.GetById(id);
        if (channel == null)
        {
            return false;
        }

        var permissions = await resolver.ForChannel(channel);
        permissions.Require(Permission.ChannelDeleteChannel);

        var deleted = await repos.Channels.DeleteById(id);
        if (deleted)
        {
            await webSocketConnector.SendDeletedChannelAsync(new WebsocketChannelDeletedPayload
            {
                Id = id,
                WorkspaceId = channel.WorkspaceId,
                IsDm = channel.IsDm,
            });
        }
        return deleted;
    }

    /// <summary>Creates a channel group (sidebar section). Requires ChannelAddChannelGroup on the workspace.</summary>
    [Authorize]
    public async Task<ChannelGroup> AddChannelGroup(
        AppRepos repos,
        IPermissionResolver resolver,
        ChannelGroupDto channelGroupDto)
    {
        var workspace = await repos.Workspaces.GetById(channelGroupDto.WorkspaceId);
        if (workspace == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var permissions = await resolver.ForWorkspace(workspace!);
        permissions.Require(Permission.ChannelAddChannelGroup);

        var channelGroup = ChannelGroup.FromDto(channelGroupDto);
        channelGroup.Id = await repos.ChannelGroups.InsertOne(channelGroup);
        return channelGroup;
    }

    /// <summary>Updates a channel group, matched by id. Requires ChannelEditChannelGroup on its workspace.</summary>
    /// <returns>The saved group, or null if it or its workspace doesn't exist.</returns>
    [Authorize]
    public async Task<ChannelGroup?> UpdateChannelGroup(
        AppRepos repos,
        IPermissionResolver resolver,
        ChannelGroup channelGroup)
    {
        var workspace = await repos.Workspaces.GetById(channelGroup.WorkspaceId);
        if (workspace == null)
        {
            return null;
        }

        var permissions = await resolver.ForWorkspace(workspace);
        permissions.Require(Permission.ChannelEditChannelGroup);

        var updated = await repos.ChannelGroups.UpdateOne(channelGroup);
        return updated ? channelGroup : null;
    }

    /// <summary>Deletes a channel group. Requires ChannelDeleteChannelGroup on its workspace.</summary>
    /// <returns>True if it existed and was deleted.</returns>
    [Authorize]
    public async Task<bool> DeleteChannelGroup(
        AppRepos repos,
        IPermissionResolver resolver,
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

        var permissions = await resolver.ForWorkspace(workspace);
        permissions.Require(Permission.ChannelDeleteChannelGroup);

        return await repos.ChannelGroups.DeleteById(id);
    }

    /// <summary>
    /// Returns the DM conversation with exactly these participants, creating it if needed. Every participant
    /// gets read/write access to it. FORBIDDEN_ACTION if there's nobody besides yourself, NOT_FOUND if a user
    /// doesn't exist.
    /// </summary>
    /// <param name="otherUserIds">The other participants. You are added automatically; duplicates are ignored.</param>
    [Authorize]
    public async Task<DirectMessage> OpenDirectMessage(
        AppRepos repos,
        IPermissionResolver resolver,
        ClaimsPrincipal principal,
        WebSocketConnector webSocketConnector,
        Guid[] otherUserIds)
    {
        var actorId = principal.GetUserGuid();
        var memberIds = ChannelMembership.DistinctMemberIds(actorId, otherUserIds ?? []);
        if (memberIds.Count < 2)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        var otherIds = memberIds.Where(id => id != actorId).ToList();
        var otherUsers = await repos.Users.GetList(user => otherIds.Contains(user.Id));
        if (otherUsers.Count != otherIds.Count)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var existing = await ChannelMembership.FindByExactMembers(repos, memberIds);
        if (existing != null)
        {
            return DirectMessage.From(existing, otherUsers.OrderBy(user => user.Nickname));
        }

        var channel = new Channel
        {
            Name = DomainSettings.DmChannelName,
            ChannelTypeId = null,
            WorkspaceId = null,
            IsDm = true,
        };
        channel.SecObjId = await SecureObjectHelper.Create(
            repos,
            new SecureObjectDescription { Kind = "channel", Name = DomainSettings.DmChannelName },
            grantAdminRole: false);
        channel.Id = await repos.Channels.InsertOne(channel);

        await ChannelMembership.InsertMembers(repos, channel.Id, memberIds);

        foreach (var memberId in memberIds)
        {
            await GrantDirectMessageParticipantAcl(repos, webSocketConnector, memberId, channel.SecObjId);
        }

        await webSocketConnector.SendNewChannelAsync(
            WebsocketChannelPayload.FromChannel(channel, memberIds));
        resolver.Invalidate();
        return DirectMessage.From(channel, otherUsers.OrderBy(user => user.Nickname));
    }

    private static async Task GrantDirectMessageParticipantAcl(
        AppRepos repos,
        WebSocketConnector webSocketConnector,
        Guid userId,
        int secObjId)
    {
        var userAcl = new UserAcl
        {
            UserId = userId,
            SecObjId = secObjId,
            Permissions = DomainSettings.DirectMessageParticipantPermissions,
        };
        await repos.UserAcls.Upsert(userAcl);
        await webSocketConnector.SendUserAclChangedAsync(WebsocketUserAclPayload.FromUserAcl(userAcl));
    }
}
