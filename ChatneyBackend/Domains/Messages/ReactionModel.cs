using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

public class ReactionInMessage
{
    [BsonElement("code")]
    [MaxLength(255)]
    public string Code { get; set; }

    [BsonElement("count")]
    public int Count { get; set; }
}

public class MessageReactionDbModel : DatabaseItem
{
    [BsonElement("_id")]
    [BsonId]
    public string Id { get; set; }

    [BsonElement("messageId")]
    public string MessageId { get; set; }

    [BsonElement("code")]
    public string Code { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}