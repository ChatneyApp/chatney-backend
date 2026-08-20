using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;

namespace ChatneyBackend.Infra.Middleware;

public readonly struct WebSocketPayloadType
{
    public string Value { get; }

    private WebSocketPayloadType(string value)
    {
        Value = value;
    }

    public static readonly WebSocketPayloadType NewReaction = new("newReaction");
    public static readonly WebSocketPayloadType DeletedReaction = new("deletedReaction");
    public static readonly WebSocketPayloadType NewMessage = new("newMessage");
    public static readonly WebSocketPayloadType DeletedMessage = new("deletedMessage");
    public static readonly WebSocketPayloadType MessageChildrenCountUpdated = new("messageChildrenCountUpdated");
    public static readonly WebSocketPayloadType EditedMessage = new("editedMessage");
    public static readonly WebSocketPayloadType NewRole = new("newRole");
    public static readonly WebSocketPayloadType UpdatedRole = new("updatedRole");
    public static readonly WebSocketPayloadType DeletedRole = new("deletedRole");
    public static readonly WebSocketPayloadType NewUserRole = new("newUserRole");
    public static readonly WebSocketPayloadType DeletedUserRole = new("deletedUserRole");
    public static readonly WebSocketPayloadType RoleAclChanged = new("roleAclChanged");
    public static readonly WebSocketPayloadType RoleAclDeleted = new("roleAclDeleted");
    public static readonly WebSocketPayloadType UserAclChanged = new("userAclChanged");
    public static readonly WebSocketPayloadType UserAclDeleted = new("userAclDeleted");
    public static readonly WebSocketPayloadType NewChannel = new("newChannel");
    public static readonly WebSocketPayloadType UpdatedChannel = new("updatedChannel");
    public static readonly WebSocketPayloadType DeletedChannel = new("deletedChannel");
    public static readonly WebSocketPayloadType NewChannelType = new("newChannelType");
    public static readonly WebSocketPayloadType UpdatedChannelType = new("updatedChannelType");
    public static readonly WebSocketPayloadType DeletedChannelType = new("deletedChannelType");
    public static readonly WebSocketPayloadType NewWorkspace = new("newWorkspace");
    public static readonly WebSocketPayloadType UpdatedWorkspace = new("updatedWorkspace");
    public static readonly WebSocketPayloadType DeletedWorkspace = new("deletedWorkspace");

    public override string ToString() => Value;

    // public static implicit operator string(WebocketPayloadType s) => s.Value;
    // public static explicit operator WebocketPayloadType(string str) =>
    //     str switch
    //     {
    //         "message" => Message,
    //         "inactive" => Inactive,
    //         "deleted" => Deleted,
    //         _ => throw new ArgumentException($"Unknown status: {str}")
    //     };
}

public class WebSocketPayloadTypeConverter : JsonConverter<WebSocketPayloadType>
{
    public override WebSocketPayloadType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException(); // implement if deserialization is needed
    }

    public override void Write(Utf8JsonWriter writer, WebSocketPayloadType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

public class WebSocketConnector
{
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
        return SendForMembersOrAllAsync(memberUserIds, WebSocketPayloadType.NewReaction, reaction);
    }
    public virtual Task DeleteReactionAsync(WebsocketReactionPayload reaction, IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(memberUserIds, WebSocketPayloadType.DeletedReaction, reaction);
    }
    #endregion

    #region Messages
    public virtual Task SendMessageAsync(MessageWithUser message)
    {
        return SendToAllAsync(WebSocketPayloadType.NewMessage, message);
    }
    public virtual Task SendNewMessageAsync(NewMessagePayload payload, IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(memberUserIds, WebSocketPayloadType.NewMessage, payload);
    }
    public virtual Task DeleteMessageAsync(DeletedMessage message, IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(memberUserIds, WebSocketPayloadType.DeletedMessage, message);
    }
    public virtual Task UpdateMessageChildrenCountAsync(
        MessageChildrenCountUpdated message,
        IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(memberUserIds, WebSocketPayloadType.MessageChildrenCountUpdated, message);
    }
    public virtual Task SendEditedMessageAsync(MessageWithUser message, IReadOnlyList<Guid>? memberUserIds)
    {
        return SendForMembersOrAllAsync(
            memberUserIds,
            WebSocketPayloadType.EditedMessage,
            new EditedMessagePayload { Message = message });
    }
    #endregion

    #region Roles
    public virtual Task SendNewRoleAsync(WebsocketRolePayload role)
    {
        return SendToAllAsync(WebSocketPayloadType.NewRole, role);
    }

    public virtual Task SendUpdatedRoleAsync(WebsocketRolePayload role)
    {
        return SendToAllAsync(WebSocketPayloadType.UpdatedRole, role);
    }

    public virtual Task SendDeletedRoleAsync(WebsocketRoleDeletedPayload role)
    {
        return SendToAllAsync(WebSocketPayloadType.DeletedRole, role);
    }
    #endregion

    #region User Roles
    public virtual Task SendNewUserRoleAsync(UserRole userRole)
    {
        return SendToUserAsync(
            userRole.UserId,
            WebSocketPayloadType.NewUserRole,
            WebsocketUserRolePayload.FromUserRole(userRole));
    }

    public virtual Task SendDeletedUserRoleAsync(WebsocketUserRoleDeletedPayload payload)
    {
        return SendToUserAsync(payload.UserId, WebSocketPayloadType.DeletedUserRole, payload);
    }
    #endregion

    #region Role Acls
    public virtual Task SendRoleAclChangedAsync(WebsocketRoleAclPayload payload)
    {
        return SendToAllAsync(WebSocketPayloadType.RoleAclChanged, payload);
    }

    public virtual Task SendRoleAclDeletedAsync(WebsocketRoleAclDeletedPayload payload)
    {
        return SendToAllAsync(WebSocketPayloadType.RoleAclDeleted, payload);
    }
    #endregion

    #region User Acls
    public virtual Task SendUserAclChangedAsync(WebsocketUserAclPayload payload)
    {
        return SendToUserAsync(payload.UserId, WebSocketPayloadType.UserAclChanged, payload);
    }

    public virtual Task SendUserAclDeletedAsync(WebsocketUserAclDeletedPayload payload)
    {
        return SendToUserAsync(payload.UserId, WebSocketPayloadType.UserAclDeleted, payload);
    }
    #endregion

    #region Channels
    public virtual Task SendNewChannelAsync(WebsocketChannelPayload channel)
    {
        return SendForMembersOrAllAsync(channel.MemberUserIds, WebSocketPayloadType.NewChannel, channel);
    }

    public virtual Task SendUpdatedChannelAsync(WebsocketChannelPayload channel)
    {
        return SendForMembersOrAllAsync(channel.MemberUserIds, WebSocketPayloadType.UpdatedChannel, channel);
    }

    public virtual Task SendDeletedChannelAsync(WebsocketChannelDeletedPayload channel)
    {
        return SendToAllAsync(WebSocketPayloadType.DeletedChannel, channel);
    }
    #endregion

    #region Channel Types
    public virtual Task SendNewChannelTypeAsync(WebsocketChannelTypePayload channelType)
    {
        return SendToAllAsync(WebSocketPayloadType.NewChannelType, channelType);
    }

    public virtual Task SendUpdatedChannelTypeAsync(WebsocketChannelTypePayload channelType)
    {
        return SendToAllAsync(WebSocketPayloadType.UpdatedChannelType, channelType);
    }

    public virtual Task SendDeletedChannelTypeAsync(WebsocketChannelTypeDeletedPayload channelType)
    {
        return SendToAllAsync(WebSocketPayloadType.DeletedChannelType, channelType);
    }
    #endregion

    #region Workspaces
    public virtual Task SendNewWorkspaceAsync(WebsocketWorkspacePayload workspace)
    {
        return SendToAllAsync(WebSocketPayloadType.NewWorkspace, workspace);
    }

    public virtual Task SendUpdatedWorkspaceAsync(WebsocketWorkspacePayload workspace)
    {
        return SendToAllAsync(WebSocketPayloadType.UpdatedWorkspace, workspace);
    }

    public virtual Task SendDeletedWorkspaceAsync(WebsocketWorkspaceDeletedPayload workspace)
    {
        return SendToAllAsync(WebSocketPayloadType.DeletedWorkspace, workspace);
    }
    #endregion

    private static bool IsSocketForUser(string socketKey, Guid userId)
    {
        var userIdString = userId.ToString();
        return socketKey == userIdString ||
               socketKey.StartsWith(userIdString + "--", StringComparison.Ordinal);
    }

    private Task SendToAllAsync(WebSocketPayloadType type, object payload)
    {
        return SendToSocketsAsync(websocketsMapping, type, payload);
    }

    private Task SendToUserAsync(Guid userId, WebSocketPayloadType type, object payload)
    {
        var userSockets = websocketsMapping
            .Where(kvp => IsSocketForUser(kvp.Key, userId))
            .ToList();

        return SendToSocketsAsync(userSockets, type, payload);
    }

    private Task SendToUsersAsync(IReadOnlyList<Guid> userIds, WebSocketPayloadType type, object payload)
    {
        var userIdSet = userIds.ToHashSet();
        var userSockets = websocketsMapping
            .Where(kvp => userIdSet.Any(userId => IsSocketForUser(kvp.Key, userId)))
            .ToList();

        return SendToSocketsAsync(userSockets, type, payload);
    }

    private Task SendForMembersOrAllAsync(
        IReadOnlyList<Guid>? memberUserIds,
        WebSocketPayloadType type,
        object payload)
    {
        if (memberUserIds is { Count: > 0 })
        {
            return SendToUsersAsync(memberUserIds, type, payload);
        }

        return SendToAllAsync(type, payload);
    }

    private async Task SendToSocketsAsync(
        IEnumerable<KeyValuePair<string, WebSocket>> sockets,
        WebSocketPayloadType type,
        object payload)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new WebSocketPayloadTypeConverter() }
        };

        var websocketPayload = new
        {
            Type = type,
            Payload = payload,
        };

        var serializedMessage = JsonSerializer.Serialize(websocketPayload, options);
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
