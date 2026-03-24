using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Roles;

public class Role : IPgDatabaseItem<int>
{
    [Column("id")]
    [MaxLength(36)]
    public int Id { get; set; }

    [Column("name")]
    [MaxLength(255)]
    public string Name { get; set; }

    [Column("is_base")]
    public bool IsBase { get; set; }

    [Column("permissions")]
    public string[] Permissions { get; set; } = [];

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
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
