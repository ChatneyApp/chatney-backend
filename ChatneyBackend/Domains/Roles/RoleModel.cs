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

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static Role FromDto(RoleCreateDto role)
    {
        return new Role()
        {
            Name = role.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void PatchFromDto(RoleUpdateDto role)
    {
        Name = role.Name;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Expression<Func<Role, bool>> MatchByKey(int key) => role => role.Id == key;

    public static int GetKey(Role record) => record.Id;
}

public class RoleCreateDto
{
    [MaxLength(255)]
    public required string Name { get; set; }
}

public class RoleUpdateDto
{
    public int Id { get; set; }

    [MaxLength(255)]
    public required string Name { get; set; }
}

public class WebsocketRolePayload
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public static WebsocketRolePayload FromRole(Role role) => new()
    {
        Id = role.Id,
        Name = role.Name,
    };
}

public class WebsocketRoleDeletedPayload
{
    public int Id { get; set; }
}
