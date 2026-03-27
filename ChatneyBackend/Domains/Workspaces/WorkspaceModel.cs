using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Workspaces;

public class Workspace : IPgKey<Workspace, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static Workspace FromDTO(WorkspaceDTO workspace)
    {
        return new Workspace()
        {
            Name = workspace.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public static Expression<Func<Workspace, bool>> MatchByKey(int key) => workspace => workspace.Id == key;
}

public class WorkspaceDTO
{
    [MaxLength(255)]
    public required string Name { get; set; }
}
