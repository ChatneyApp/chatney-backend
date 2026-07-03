using ChatneyBackend.Domains.Messages;
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
}
