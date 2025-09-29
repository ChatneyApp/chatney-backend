using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ChatneyBackend.Domains.Messages;

namespace ChatneyBackend.Infra.Middleware;

public class WebSocketConnector
{
    public Dictionary<string, WebSocket> websocketsMapping = new Dictionary<string, WebSocket>();
    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSockets();  // Enable WebSocket middleware

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest && context.Request.Query.TryGetValue("userId", out var userId))
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await HandleWebSocketAsync(webSocket);
                    websocketsMapping.Add(userId.ToString(), webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400; // Bad Request
                }
            }
            else
            {
                await next();
            }
        });
    }

    /**
     * Receiving messages from ws  
     */
    private async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        byte[] buffer = new byte[1024 * 4];  // Buffer for incoming messages
        WebSocketReceiveResult result;

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                // Receive message from the client
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received JSON: {jsonMessage}");

                    // Deserialize the received message into an object
                    var receivedObject = JsonSerializer.Deserialize<MessageDTO>(jsonMessage);

                    if (receivedObject == null)
                    {
                        throw new Exception("Message data is corrupted");
                    }

                    var messageModel = Message.FromDTO(receivedObject, "sdfsdfsd");


                    // Serialize the response object into JSON
                    string jsonResponse = JsonSerializer.Serialize(messageModel);

                    // Send the response back to the client
                    await SendMessageAsync(webSocket, jsonResponse);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    // Sending messages to ws
    private async Task SendMessageAsync(WebSocket webSocket, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task sendMessage(Message message)
    {
        var serializedMessage = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(serializedMessage);
        var segment = new ArraySegment<byte>(buffer);
        var deadSocketsIds = new HashSet<string>();

        foreach (var kvp in websocketsMapping)
        {
            WebSocket socket = kvp.Value;

            if (socket != null && socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                deadSocketsIds.Add(kvp.Key);
            }
        }

        foreach (var deadSocketId in deadSocketsIds)
        {
            websocketsMapping.Remove(deadSocketId);
        }
    }
}