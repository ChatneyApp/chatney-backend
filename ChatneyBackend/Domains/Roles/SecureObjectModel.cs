using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatneyBackend.Infra;
using RepoDb;
using RepoDb.Attributes;
using RepoDb.Interfaces;
using RepoDb.Options;

namespace ChatneyBackend.Domains.Roles;

public class SecureObjectDescription
{
    [JsonPropertyName("kind")]
    public required string Kind { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

/// <summary>
/// RepoDb 1.13.1 has no built-in handler for POCO-typed jsonb columns: without this, a SELECT would
/// try to cast the raw jsonb value straight into <see cref="SecureObjectDescription"/> and fail. The
/// write side goes through <c>SecureObjectHelper.Create</c>'s explicit ::jsonb SQL instead (see
/// InsertOne's InvalidCastException regression), but this handler still needs to serialize on Set
/// because RepoDb also applies property handlers to UpdateAsync/MergeAsync calls.
/// </summary>
public class SecureObjectDescriptionHandler : IPropertyHandler<string?, SecureObjectDescription?>
{
    public SecureObjectDescription? Get(string? input, PropertyHandlerGetOptions options) =>
        input == null ? null : JsonSerializer.Deserialize<SecureObjectDescription>(input);

    public string? Set(SecureObjectDescription? input, PropertyHandlerSetOptions options) =>
        input == null ? null : JsonSerializer.Serialize(input);
}

public class SecureObject : IPgKey<SecureObject, int>
{
    [Primary]
    [Identity]
    [Map("id")]
    public int Id { get; set; }

    [Map("description")]
    [PropertyHandler(typeof(SecureObjectDescriptionHandler))]
    public SecureObjectDescription? Description { get; set; }

    public static Expression<Func<SecureObject, bool>> MatchByKey(int key) => obj => obj.Id == key;

    public static int GetKey(SecureObject record) => record.Id;
}
