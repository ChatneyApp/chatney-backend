using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Roles;

public class RoleSettings
{
    [BsonElement("base")]
    public bool Base { get; set; }
}

public class Role
{
    [BsonElement("_id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [BsonElement("settings")]
    public RoleSettings Settings { get; set; }

    [BsonElement("permissions")]
    public List<string> Permissions { get; set; }
}
