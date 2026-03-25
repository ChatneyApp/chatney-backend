using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Roles;

public class Role : IPgDatabaseItem<int>
{
    [Primary]
    [Identity]
    [Map("id")]
    [MaxLength(36)]
    public int Id { get; set; }

    [Map("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [Map("is_base")]
    public bool IsBase { get; set; }

    [Map("permissions")]
    public string[] Permissions { get; set; } = [];

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static Role FromDTO(RoleDto role)
    {
        return new Role()
        {
            Name = role.Name,
            Permissions = role.Permissions.ToArray(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsBase = role.IsBase,
        };
    }
}

public class RoleDto
{
    [MaxLength(255)]
    public string Name { get; set; }

    public bool IsBase { get; set; }

    public List<string> Permissions { get; set; }
}
