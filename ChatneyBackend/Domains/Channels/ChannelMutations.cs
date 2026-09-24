using System.Security.Claims;
using ChatneyBackend.Domains.Permissions;
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
        if (channelType == null || channelType.Key == DomainSettings.DmChannelTypeKey)
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

        var dmChannelType = await repos.ChannelTypes.GetOne(channelType =>
            channelType.Key == DomainSettings.DmChannelTypeKey);
        if (dmChannelType == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var existing = await ChannelMembership.FindByExactMembers(repos, dmChannelType!.Id, memberIds);
        if (existing != null)
        {
            return DirectMessage.From(existing, otherUsers.OrderBy(user => user.Nickname));
        }

        var channel = new Channel
        {
            Name = DomainSettings.DmChannelName,
            ChannelTypeId = dmChannelType.Id,
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
