using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra.Middleware;

namespace ChatneyBackend.Tests.Support;

public class RecordingWebSocketConnector : WebSocketConnector
{
    public List<NewMessagePayload> NewMessages { get; } = [];
    public List<MessageWithUser> EditedMessages { get; } = [];
    public List<DeletedMessage> DeletedMessages { get; } = [];
    public List<MessageChildrenCountUpdated> ChildrenCountUpdates { get; } = [];
    public List<WebsocketReactionPayload> AddedReactions { get; } = [];
    public List<WebsocketReactionPayload> DeletedReactions { get; } = [];
    public List<WebsocketRolePayload> NewRoles { get; } = [];
    public List<WebsocketRolePayload> UpdatedRoles { get; } = [];
    public List<WebsocketRoleDeletedPayload> DeletedRoles { get; } = [];
    public List<WebsocketUserRolePayload> NewUserRoles { get; } = [];
    public List<WebsocketUserRoleDeletedPayload> DeletedUserRoles { get; } = [];
    public List<WebsocketRoleAclPayload> ChangedRoleAcls { get; } = [];
    public List<WebsocketRoleAclDeletedPayload> DeletedRoleAcls { get; } = [];
    public List<WebsocketUserAclPayload> ChangedUserAcls { get; } = [];
    public List<WebsocketUserAclDeletedPayload> DeletedUserAcls { get; } = [];
    public List<WebsocketChannelPayload> NewChannels { get; } = [];
    public List<WebsocketChannelPayload> UpdatedChannels { get; } = [];
    public List<WebsocketChannelDeletedPayload> DeletedChannels { get; } = [];
    public List<WebsocketChannelTypePayload> NewChannelTypes { get; } = [];
    public List<WebsocketChannelTypePayload> UpdatedChannelTypes { get; } = [];
    public List<WebsocketChannelTypeDeletedPayload> DeletedChannelTypes { get; } = [];
    public List<WebsocketWorkspacePayload> NewWorkspaces { get; } = [];
    public List<WebsocketWorkspacePayload> UpdatedWorkspaces { get; } = [];
    public List<WebsocketWorkspaceDeletedPayload> DeletedWorkspaces { get; } = [];

    public override Task SendNewMessageAsync(NewMessagePayload payload)
    {
        NewMessages.Add(payload);
        return Task.CompletedTask;
    }

    public override Task SendEditedMessageAsync(MessageWithUser message)
    {
        EditedMessages.Add(message);
        return Task.CompletedTask;
    }

    public override Task DeleteMessageAsync(DeletedMessage message)
    {
        DeletedMessages.Add(message);
        return Task.CompletedTask;
    }

    public override Task UpdateMessageChildrenCountAsync(MessageChildrenCountUpdated message)
    {
        ChildrenCountUpdates.Add(message);
        return Task.CompletedTask;
    }

    public override Task AddReactionAsync(WebsocketReactionPayload reaction)
    {
        AddedReactions.Add(reaction);
        return Task.CompletedTask;
    }

    public override Task DeleteReactionAsync(WebsocketReactionPayload reaction)
    {
        DeletedReactions.Add(reaction);
        return Task.CompletedTask;
    }

    public override Task SendNewRoleAsync(WebsocketRolePayload role)
    {
        NewRoles.Add(role);
        return Task.CompletedTask;
    }

    public override Task SendUpdatedRoleAsync(WebsocketRolePayload role)
    {
        UpdatedRoles.Add(role);
        return Task.CompletedTask;
    }

    public override Task SendDeletedRoleAsync(WebsocketRoleDeletedPayload role)
    {
        DeletedRoles.Add(role);
        return Task.CompletedTask;
    }

    public override Task SendNewUserRoleAsync(UserRole userRole)
    {
        NewUserRoles.Add(WebsocketUserRolePayload.FromUserRole(userRole));
        return Task.CompletedTask;
    }

    public override Task SendDeletedUserRoleAsync(WebsocketUserRoleDeletedPayload payload)
    {
        DeletedUserRoles.Add(payload);
        return Task.CompletedTask;
    }

    public override Task SendRoleAclChangedAsync(WebsocketRoleAclPayload payload)
    {
        ChangedRoleAcls.Add(payload);
        return Task.CompletedTask;
    }

    public override Task SendRoleAclDeletedAsync(WebsocketRoleAclDeletedPayload payload)
    {
        DeletedRoleAcls.Add(payload);
        return Task.CompletedTask;
    }

    public override Task SendUserAclChangedAsync(WebsocketUserAclPayload payload)
    {
        ChangedUserAcls.Add(payload);
        return Task.CompletedTask;
    }

    public override Task SendUserAclDeletedAsync(WebsocketUserAclDeletedPayload payload)
    {
        DeletedUserAcls.Add(payload);
        return Task.CompletedTask;
    }

    public override Task SendNewChannelAsync(WebsocketChannelPayload channel)
    {
        NewChannels.Add(channel);
        return Task.CompletedTask;
    }

    public override Task SendUpdatedChannelAsync(WebsocketChannelPayload channel)
    {
        UpdatedChannels.Add(channel);
        return Task.CompletedTask;
    }

    public override Task SendDeletedChannelAsync(WebsocketChannelDeletedPayload channel)
    {
        DeletedChannels.Add(channel);
        return Task.CompletedTask;
    }

    public override Task SendNewChannelTypeAsync(WebsocketChannelTypePayload channelType)
    {
        NewChannelTypes.Add(channelType);
        return Task.CompletedTask;
    }

    public override Task SendUpdatedChannelTypeAsync(WebsocketChannelTypePayload channelType)
    {
        UpdatedChannelTypes.Add(channelType);
        return Task.CompletedTask;
    }

    public override Task SendDeletedChannelTypeAsync(WebsocketChannelTypeDeletedPayload channelType)
    {
        DeletedChannelTypes.Add(channelType);
        return Task.CompletedTask;
    }

    public override Task SendNewWorkspaceAsync(WebsocketWorkspacePayload workspace)
    {
        NewWorkspaces.Add(workspace);
        return Task.CompletedTask;
    }

    public override Task SendUpdatedWorkspaceAsync(WebsocketWorkspacePayload workspace)
    {
        UpdatedWorkspaces.Add(workspace);
        return Task.CompletedTask;
    }

    public override Task SendDeletedWorkspaceAsync(WebsocketWorkspaceDeletedPayload workspace)
    {
        DeletedWorkspaces.Add(workspace);
        return Task.CompletedTask;
    }
}
