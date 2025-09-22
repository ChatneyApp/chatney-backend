using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Workspaces;

public class Workspace : DatabaseItem
{
    [BsonElement("_id")]
    [BsonId]
    [MaxLength(36)]
    public string Id { get; set; }

    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    public static Workspace FromDTO(WorkspaceDTO workspace)
    {
        return new Workspace()
        {
            Id = Guid.NewGuid().ToString(),
            Name = workspace.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}

public class WorkspaceDTO
{
    [BsonElement("name")]
    [MaxLength(255)]
    public string Name { get; set; }
}
