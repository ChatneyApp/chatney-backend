using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Configs;

public class Config : DatabaseItem
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [BsonElement("value")]
    [MaxLength(2048)]
    public string Value { get; set; }

    [BsonElement("type")]
    [MaxLength(255)]
    public string? Type { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    public static Config FromDTO(ConfigDTO role)
    {
        return new Config()
        {
            Id = Guid.NewGuid().ToString(),
            Name = role.Name,
            Value = role.Value,
            Type = role.Type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}

public class ConfigDTO
{
    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [BsonElement("value")]
    [MaxLength(2048)]
    public string Value { get; set; }

    [BsonElement("type")]
    [MaxLength(255)]
    public string? Type { get; set; }
}
