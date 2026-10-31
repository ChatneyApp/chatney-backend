using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;

namespace ChatneyBackend.Infra.Middleware;

public interface IWebSocketEvent
{
    string Name { get; }
    Type PayloadType { get; }
}

public sealed record WebSocketEvent<TPayload>(string Name) : IWebSocketEvent
{
    public Type PayloadType => typeof(TPayload);
}

// WebSocketSchemaExporter reads these fields to generate the frontend's event types.
public static class WebSocketEvents
{
    public static readonly WebSocketEvent<WebsocketReactionPayload> NewReaction = new("newReaction");
    public static readonly WebSocketEvent<WebsocketReactionPayload> DeletedReaction = new("deletedReaction");
    public static readonly WebSocketEvent<NewMessagePayload> NewMessage = new("newMessage");
    public static readonly WebSocketEvent<DeletedMessage> DeletedMessage = new("deletedMessage");
    public static readonly WebSocketEvent<MessageChildrenCountUpdated> MessageChildrenCountUpdated = new("messageChildrenCountUpdated");
    public static readonly WebSocketEvent<EditedMessagePayload> EditedMessage = new("editedMessage");
    public static readonly WebSocketEvent<WebsocketRolePayload> NewRole = new("newRole");
    public static readonly WebSocketEvent<WebsocketRolePayload> UpdatedRole = new("updatedRole");
    public static readonly WebSocketEvent<WebsocketRoleDeletedPayload> DeletedRole = new("deletedRole");
    public static readonly WebSocketEvent<WebsocketUserRolePayload> NewUserRole = new("newUserRole");
    public static readonly WebSocketEvent<WebsocketUserRoleDeletedPayload> DeletedUserRole = new("deletedUserRole");
    public static readonly WebSocketEvent<WebsocketRoleAclPayload> RoleAclChanged = new("roleAclChanged");
    public static readonly WebSocketEvent<WebsocketRoleAclDeletedPayload> RoleAclDeleted = new("roleAclDeleted");
    public static readonly WebSocketEvent<WebsocketUserAclPayload> UserAclChanged = new("userAclChanged");
    public static readonly WebSocketEvent<WebsocketUserAclDeletedPayload> UserAclDeleted = new("userAclDeleted");
    public static readonly WebSocketEvent<WebsocketChannelPayload> NewChannel = new("newChannel");
    public static readonly WebSocketEvent<WebsocketChannelPayload> UpdatedChannel = new("updatedChannel");
    public static readonly WebSocketEvent<WebsocketChannelDeletedPayload> DeletedChannel = new("deletedChannel");
    public static readonly WebSocketEvent<WebsocketChannelTypePayload> NewChannelType = new("newChannelType");
    public static readonly WebSocketEvent<WebsocketChannelTypePayload> UpdatedChannelType = new("updatedChannelType");
    public static readonly WebSocketEvent<WebsocketChannelTypeDeletedPayload> DeletedChannelType = new("deletedChannelType");
    public static readonly WebSocketEvent<WebsocketWorkspacePayload> NewWorkspace = new("newWorkspace");
    public static readonly WebSocketEvent<WebsocketWorkspacePayload> UpdatedWorkspace = new("updatedWorkspace");
    public static readonly WebSocketEvent<WebsocketWorkspaceDeletedPayload> DeletedWorkspace = new("deletedWorkspace");
    public static readonly WebSocketEvent<WebsocketChannelGroupPayload> NewChannelGroup = new("newChannelGroup");
    public static readonly WebSocketEvent<WebsocketChannelGroupPayload> UpdatedChannelGroup = new("updatedChannelGroup");
    public static readonly WebSocketEvent<WebsocketChannelGroupsOrderPayload> ChannelGroupsOrderUpdated = new("channelGroupsOrderUpdated");
    public static readonly WebSocketEvent<WebsocketChannelGroupDeletedPayload> DeletedChannelGroup = new("deletedChannelGroup");
    public static readonly WebSocketEvent<WebsocketUserProfilePayload> UpdatedUserProfile = new("updatedUserProfile");
    public static readonly WebSocketEvent<WebsocketConfigPayload> UpdatedConfig = new("updatedConfig");
}

public class WebSocketConnector
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    private readonly ConcurrentDictionary<string, WebSocket> websocketsMapping = new();

    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSockets();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var userId = context.Request.Query["userId"].ToString();
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("userId is required");
                        return;
                    }

                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    websocketsMapping[userId] = webSocket;

                    Console.WriteLine($"User {userId} connected.");
                    await HandleWebSocketAsync(userId, webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next();
            }
        });
    }

    private async Task HandleWebSocketAsync(string userId, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // TODO: review if this is ever used?
                    var jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received from {userId}: {jsonMessage}");

                    var receivedObject = JsonSerializer.Deserialize<MessageDto>(jsonMessage);

                    if (receivedObject == null)
                    {
                        throw new Exception("Invalid message");
                    }

                    var messageModel = Message.FromDto(receivedObject, Guid.Parse(userId));

                    var jsonResponse = JsonSerializer.Serialize(messageModel);
                    await SendMessageAsync(webSocket, jsonResponse);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"WebSocket for {userId} closed.");
                    websocketsMapping.TryRemove(userId, out _);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error for {userId}: {ex.Message}");
            websocketsMapping.TryRemove(userId, out _);
        }
    }

    private async Task SendMessageAsync(WebSocket webSocket, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    #region Message Reactions
    public virtual Task AddReactionAsync(WebsocketReactionPayload reaction, IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(memberUserIds, WebSocketEvents.NewReaction, reaction);
    }
    public virtual Task DeleteReactionAsync(WebsocketReactionPayload reaction, IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(memberUserIds, WebSocketEvents.DeletedReaction, reaction);
    }
    #endregion

    #region Messages
    public virtual Task SendNewMessageAsync(NewMessagePayload payload, IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(memberUserIds, WebSocketEvents.NewMessage, payload);
    }
    public virtual Task DeleteMessageAsync(DeletedMessage message, IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(memberUserIds, WebSocketEvents.DeletedMessage, message);
    }
    public virtual Task UpdateMessageChildrenCountAsync(
        MessageChildrenCountUpdated message,
        IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(memberUserIds, WebSocketEvents.MessageChildrenCountUpdated, message);
    }
    public virtual Task SendEditedMessageAsync(MessageWithUser message, IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(
            memberUserIds,
            WebSocketEvents.EditedMessage,
            new EditedMessagePayload { Message = message });
    }
    #endregion

    #region Roles
    public virtual Task SendNewRoleAsync(WebsocketRolePayload role)
    {
        return SendToAllAsync(WebSocketEvents.NewRole, role);
    }

    public virtual Task SendUpdatedRoleAsync(WebsocketRolePayload role)
    {
        return SendToAllAsync(WebSocketEvents.UpdatedRole, role);
    }

    public virtual Task SendDeletedRoleAsync(WebsocketRoleDeletedPayload role)
    {
        return SendToAllAsync(WebSocketEvents.DeletedRole, role);
    }
    #endregion

    #region User Roles
    public virtual Task SendNewUserRoleAsync(UserRole userRole)
    {
        return SendToUserAsync(
            userRole.UserId,
            WebSocketEvents.NewUserRole,
            WebsocketUserRolePayload.FromUserRole(userRole));
    }

    public virtual Task SendDeletedUserRoleAsync(WebsocketUserRoleDeletedPayload payload)
    {
        return SendToUserAsync(payload.UserId, WebSocketEvents.DeletedUserRole, payload);
    }
    #endregion

    #region Role Acls
    public virtual Task SendRoleAclChangedAsync(WebsocketRoleAclPayload payload)
    {
        return SendToAllAsync(WebSocketEvents.RoleAclChanged, payload);
    }

    public virtual Task SendRoleAclDeletedAsync(WebsocketRoleAclDeletedPayload payload)
    {
        return SendToAllAsync(WebSocketEvents.RoleAclDeleted, payload);
    }
    #endregion

    #region User Acls
    public virtual Task SendUserAclChangedAsync(WebsocketUserAclPayload payload)
    {
        return SendToUserAsync(payload.UserId, WebSocketEvents.UserAclChanged, payload);
    }

    public virtual Task SendUserAclDeletedAsync(WebsocketUserAclDeletedPayload payload)
    {
        return SendToUserAsync(payload.UserId, WebSocketEvents.UserAclDeleted, payload);
    }
    #endregion

    #region Channels
    public virtual Task SendNewChannelAsync(WebsocketChannelPayload channel)
    {
        return SendForMembersOrAllAsync(channel.MemberUserIds, WebSocketEvents.NewChannel, channel);
    }

    public virtual Task SendUpdatedChannelAsync(WebsocketChannelPayload channel)
    {
        return SendForMembersOrAllAsync(channel.MemberUserIds, WebSocketEvents.UpdatedChannel, channel);
    }

    public virtual Task SendDeletedChannelAsync(WebsocketChannelDeletedPayload channel)
    {
        return SendToAllAsync(WebSocketEvents.DeletedChannel, channel);
    }
    #endregion

    #region Channel Types
    public virtual Task SendNewChannelTypeAsync(WebsocketChannelTypePayload channelType)
    {
        return SendToAllAsync(WebSocketEvents.NewChannelType, channelType);
    }

    public virtual Task SendUpdatedChannelTypeAsync(WebsocketChannelTypePayload channelType)
    {
        return SendToAllAsync(WebSocketEvents.UpdatedChannelType, channelType);
    }

    public virtual Task SendDeletedChannelTypeAsync(WebsocketChannelTypeDeletedPayload channelType)
    {
        return SendToAllAsync(WebSocketEvents.DeletedChannelType, channelType);
    }
    #endregion

    #region Workspaces
    public virtual Task SendNewWorkspaceAsync(WebsocketWorkspacePayload workspace)
    {
        return SendToAllAsync(WebSocketEvents.NewWorkspace, workspace);
    }

    public virtual Task SendUpdatedWorkspaceAsync(WebsocketWorkspacePayload workspace)
    {
        return SendToAllAsync(WebSocketEvents.UpdatedWorkspace, workspace);
    }

    public virtual Task SendDeletedWorkspaceAsync(WebsocketWorkspaceDeletedPayload workspace)
    {
        return SendToAllAsync(WebSocketEvents.DeletedWorkspace, workspace);
    }
    #endregion

    #region Channel Groups
    public virtual Task SendNewChannelGroupAsync(WebsocketChannelGroupPayload channelGroup)
    {
        return SendToAllAsync(WebSocketEvents.NewChannelGroup, channelGroup);
    }

    public virtual Task SendUpdatedChannelGroupAsync(WebsocketChannelGroupPayload channelGroup)
    {
        return SendToAllAsync(WebSocketEvents.UpdatedChannelGroup, channelGroup);
    }

    public virtual Task SendChannelGroupsOrderUpdatedAsync(WebsocketChannelGroupsOrderPayload payload)
    {
        return SendToAllAsync(WebSocketEvents.ChannelGroupsOrderUpdated, payload);
    }

    public virtual Task SendDeletedChannelGroupAsync(WebsocketChannelGroupDeletedPayload channelGroup)
    {
        return SendToAllAsync(WebSocketEvents.DeletedChannelGroup, channelGroup);
    }
    #endregion

    #region User Profiles
    public virtual Task SendUpdatedUserProfileAsync(WebsocketUserProfilePayload profile)
    {
        return SendToAllAsync(WebSocketEvents.UpdatedUserProfile, profile);
    }
    #endregion

    #region Configs
    public virtual Task SendUpdatedConfigAsync(WebsocketConfigPayload config)
    {
        return SendToAllAsync(WebSocketEvents.UpdatedConfig, config);
    }
    #endregion

    private static bool IsSocketForUser(string socketKey, Guid userId)
    {
        var userIdString = userId.ToString();
        return socketKey == userIdString ||
               socketKey.StartsWith(userIdString + "--", StringComparison.Ordinal);
    }

    private Task SendToAllAsync<TPayload>(WebSocketEvent<TPayload> webSocketEvent, TPayload payload)
    {
        return SendToSocketsAsync(websocketsMapping, webSocketEvent, payload);
    }

    private Task SendToUserAsync<TPayload>(Guid userId, WebSocketEvent<TPayload> webSocketEvent, TPayload payload)
    {
        var userSockets = websocketsMapping
            .Where(kvp => IsSocketForUser(kvp.Key, userId))
            .ToList();

        return SendToSocketsAsync(userSockets, webSocketEvent, payload);
    }

    private Task SendToUsersAsync<TPayload>(
        IReadOnlyList<Guid> userIds,
        WebSocketEvent<TPayload> webSocketEvent,
        TPayload payload)
    {
        var userIdSet = userIds.ToHashSet();
        var userSockets = websocketsMapping
            .Where(kvp => userIdSet.Any(userId => IsSocketForUser(kvp.Key, userId)))
            .ToList();

        return SendToSocketsAsync(userSockets, webSocketEvent, payload);
    }

    private Task SendForMembersOrAllAsync<TPayload>(
        IReadOnlyList<Guid>? memberUserIds,
        WebSocketEvent<TPayload> webSocketEvent,
        TPayload payload)
    {
        if (memberUserIds is { Count: > 0 })
        {
            return SendToUsersAsync(memberUserIds, webSocketEvent, payload);
        }

        return SendToAllAsync(webSocketEvent, payload);
    }

    private async Task SendToSocketsAsync<TPayload>(
        IEnumerable<KeyValuePair<string, WebSocket>> sockets,
        WebSocketEvent<TPayload> webSocketEvent,
        TPayload payload)
    {
        var websocketPayload = new
        {
            Type = webSocketEvent.Name,
            Payload = payload,
        };

        var serializedMessage = JsonSerializer.Serialize(websocketPayload, SerializerOptions);
        var buffer = Encoding.UTF8.GetBytes(serializedMessage);
        var segment = new ArraySegment<byte>(buffer);
        var deadSockets = new List<string>();

        foreach (var kvp in sockets)
        {
            var socket = kvp.Value;

            if (socket?.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    deadSockets.Add(kvp.Key);
                }
            }
            else
            {
                Console.WriteLine($"Socket {kvp.Key} is closed.");
                deadSockets.Add(kvp.Key);
            }
        }

        foreach (var deadSocket in deadSockets)
        {
            websocketsMapping.TryRemove(deadSocket, out _);
        }
    }
}
