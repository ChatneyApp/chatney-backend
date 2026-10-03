using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Configs;

/// <summary>A system-wide configuration value.</summary>
public class Config : IPgKey<Config, int>, IPgTimestamped
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    /// <summary>Unique dotted key, e.g. "messages.sendCooldown".</summary>
    [Map("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    /// <summary>Value serialized as a string; parse according to type.</summary>
    [Map("value")]
    [MaxLength(2048)]
    public required string Value { get; set; }

    /// <summary>How to parse value: "string", "int" or "string[]" (comma-separated).</summary>
    [Map("type")]
    [MaxLength(255)]
    public string? Type { get; set; }

    [Map("created_at")]
    [GraphQLIgnore]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    [GraphQLIgnore]
    public DateTime UpdatedAt { get; set; }

    public static Expression<Func<Config, bool>> MatchByKey(int key) => config => config.Id == key;

    public static int GetKey(Config record) => record.Id;
}
