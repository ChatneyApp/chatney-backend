using MongoDB.Bson.Serialization.Attributes;

public class MessageReaction : DatabaseItem
{
    [BsonElement("_id")]
    [BsonId]
    public required string Id { get; set; }

    [BsonElement("messageId")]
    public required string MessageId { get; set; }

    [BsonElement("code")]
    public required string Code { get; set; }

    [BsonElement("userId")]
    public required string UserId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public class WebsocketReactionPayload
{
    public required string Code { get; set; }
    public required string UserId { get; set; }
    public required string MessageId { get; set; }
    public required string ChannelId { get; set; }
}
