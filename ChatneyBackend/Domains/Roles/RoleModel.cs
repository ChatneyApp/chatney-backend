using System.ComponentModel.DataAnnotations;
using ChatneyBackend.Infra;
using RepoDb.Attributes;
using System.Linq.Expressions;

namespace ChatneyBackend.Domains.Roles;

public class Role : IPgKey<Role, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

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

    public static Expression<Func<Role, bool>> MatchByKey(int key) => role => role.Id == key;
}

public class RoleDto
{
    [MaxLength(255)]
    public string Name { get; set; }

    public bool IsBase { get; set; }

    public List<string> Permissions { get; set; }
}
