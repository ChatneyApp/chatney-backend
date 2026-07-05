using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
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
    public List<WebsocketUserRolePayload> UpdatedUserRoles { get; } = [];
    public List<WebsocketUserRoleDeletedPayload> DeletedUserRoles { get; } = [];

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

    public override Task SendUpdatedUserRoleAsync(UserRole userRole)
    {
        UpdatedUserRoles.Add(WebsocketUserRolePayload.FromUserRole(userRole));
        return Task.CompletedTask;
    }

    public override Task SendDeletedUserRoleAsync(WebsocketUserRoleDeletedPayload payload)
    {
        DeletedUserRoles.Add(payload);
        return Task.CompletedTask;
    }
}
