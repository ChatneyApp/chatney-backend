using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatneyBackend.Domains.Messages;

namespace ChatneyBackend.Infra.Middleware;

public readonly struct WebSocketPayloadType
{
    public string Value { get; }

    private WebSocketPayloadType(string value)
    {
        Value = value;
    }

    public static readonly WebSocketPayloadType NewMessage = new("newMessage");
    public static readonly WebSocketPayloadType Inactive = new("inactive");
    public static readonly WebSocketPayloadType Deleted = new("deleted");

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
                    var jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received from {userId}: {jsonMessage}");

                    var receivedObject = JsonSerializer.Deserialize<MessageDTO>(jsonMessage);

                    if (receivedObject == null)
                    {
                        throw new Exception("Invalid message");
                    }

                    var messageModel = Message.FromDTO(receivedObject, "ServerGeneratedId");

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

    public async Task SendMessageAsync(MessageWithUser message)
    {
        await SendToAllAsync(WebSocketPayloadType.NewMessage, message);
    }

    private async Task SendToAllAsync(WebSocketPayloadType type, Object payload)
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

        Console.WriteLine($"Broadcasting: {serializedMessage}");

        foreach (var kvp in websocketsMapping)
        {
            var socket = kvp.Value;

            if (socket?.State == WebSocketState.Open)
            {
                try
                {
                    Console.WriteLine($"Sending to {kvp.Key}");
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

        Console.WriteLine("cleaning up");

        foreach (var deadSocket in deadSockets)
        {
            websocketsMapping.TryRemove(deadSocket, out _);
        }
    }
}
