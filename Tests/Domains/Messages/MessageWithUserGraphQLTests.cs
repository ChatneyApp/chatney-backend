using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Infra.Middleware;
using System.Text.Json;

namespace ChatneyBackend.Tests.Domains.Messages;

public class MessageWithUserJsonTest
{
    public class MessageWithUserQuery
    {
        public MessageWithUser Message() => new()
        {
            Id = "message-id",
            ChannelId = "channel-id",
            UserId = "user-id",
            Content = "content",
            AttachmentIds = ["attachment-id"],
            UrlPreviewIds = ["preview-id"],
            Status = "sent",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Reactions = [],
            ParentId = null,
            ChildrenCount = 0,
            User = new MessageUser
            {
                Id = "user-id",
                Name = "User",
                AvatarUrl = null
            },
            MyReactions = [],
            UrlPreviews = [],
            Attachments = []
        };
    }

    [Fact]
    public void MessageWithUser_WebSocketPayload_DoesNotSerialize_InternalIdFields()
    {
        var message = new MessageWithUserQuery().Message();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new WebSocketPayloadTypeConverter() }
        };
        var websocketPayload = new
        {
            Type = WebSocketPayloadType.NewMessage,
            Payload = message
        };
        var json = JsonSerializer.Serialize(websocketPayload, options);

        Assert.DoesNotContain("\"attachmentIds\"", json);
        Assert.DoesNotContain("\"urlPreviewIds\"", json);
        Assert.Contains("\"type\":\"newMessage\"", json);
        Assert.Contains("\"payload\":", json);
        Assert.Contains("\"attachments\"", json);
        Assert.Contains("\"urlPreviews\"", json);
    }
}
