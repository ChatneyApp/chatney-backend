using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ChatneyBackend.Domains.Messages;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.WebSockets;

namespace ChatneyBackend.Infra.Middleware;

public class WebSocketConfigurator
{
    private readonly MessagesDomainService messagesService;
    public WebSocketConfigurator(MessagesDomainService messagesService)
    {
        this.messagesService = messagesService;
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSockets();  // Enable WebSocket middleware

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var query = context.Request.QueryString;

                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await HandleWebSocketAsync(webSocket);
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

                    var messageModel = Message.FromDTO(receivedObject);

                    var responseObject = await messagesService.AddMessage(messageModel);

                    // Serialize the response object into JSON
                    string jsonResponse = JsonSerializer.Serialize(responseObject);

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

    // Method to send a JSON message to the client
    private async Task SendMessageAsync(WebSocket webSocket, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}