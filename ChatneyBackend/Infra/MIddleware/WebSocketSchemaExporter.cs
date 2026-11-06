using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;

namespace ChatneyBackend.Infra.Middleware;

// Writes the socket contract as JSON Schema, using the options the socket serializes with.
// The frontend generates its event types from this file: see "make gen-schema".
public static class WebSocketSchemaExporter
{
    private static readonly JsonSerializerOptions OutputOptions = new() { WriteIndented = true, NewLine = "\n" };

    public static async Task ExportAsync(string outputPath)
    {
        var definitions = new SortedDictionary<string, JsonNode?>(StringComparer.Ordinal);
        var exporterOptions = new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = true,
            TransformSchemaNode = (context, schema) => MoveToDefinitions(context, schema, definitions),
        };

        var events = typeof(WebSocketEvents)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (IWebSocketEvent)field.GetValue(null)!)
            .OrderBy(webSocketEvent => webSocketEvent.Name, StringComparer.Ordinal)
            .ToList();

        var properties = new JsonObject();

        foreach (var webSocketEvent in events)
        {
            properties[webSocketEvent.Name] = WebSocketConnector.SerializerOptions
                .GetJsonSchemaAsNode(webSocketEvent.PayloadType, exporterOptions);
        }

        var schema = new JsonObject
        {
            ["title"] = "WebSocketEvents",
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["required"] = ToJsonArray(events.Select(webSocketEvent => webSocketEvent.Name)),
            ["properties"] = properties,
            ["$defs"] = new JsonObject(definitions),
        };

        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(outputPath))!);
        await File.WriteAllTextAsync(outputPath, schema.ToJsonString(OutputOptions) + "\n");
    }

    // The exporter inlines a type everywhere it's used, which would generate Attachment, Attachment1, ...
    private static JsonNode MoveToDefinitions(
        JsonSchemaExporterContext context,
        JsonNode schema,
        IDictionary<string, JsonNode?> definitions)
    {
        var definition = schema.AsObject();

        // The exporter also counts a property without a setter as nullable, but this schema only describes what the server writes.
        if (context.PropertyInfo is { IsGetNullable: false } && definition["type"] is JsonArray types)
        {
            definition["type"] = types.First(type => type!.GetValue<string>() != "null")!.DeepClone();
        }

        var type = context.TypeInfo.Type;
        var isObject = context.TypeInfo.Kind == JsonTypeInfoKind.Object;

        if (!isObject && !type.IsEnum)
        {
            return definition;
        }

        var isNullable = definition["type"] is JsonArray;
        definition["title"] = type.Name;

        if (isObject)
        {
            // The server writes every property, but the exporter only requires C# `required` members.
            definition["type"] = "object";
            definition["additionalProperties"] = false;
            definition["required"] = ToJsonArray(definition["properties"]!.AsObject().Select(property => property.Key));
        }

        definitions[type.Name] = definition;

        var reference = new JsonObject { ["$ref"] = $"#/$defs/{type.Name}" };

        if (!isNullable)
        {
            return reference;
        }

        return new JsonObject { ["anyOf"] = new JsonArray(reference, new JsonObject { ["type"] = "null" }) };
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values) =>
        new(values.Select(value => (JsonNode?)JsonValue.Create(value)).ToArray());
}
