using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonPlaceholder.ApiTesting;

public static class ApiJson
{
    /// <summary>
    /// Strict on purpose: no "1" -> 1 coercion, no case-insensitive names, no null for non-nullable members.
    /// A contract test should fail on such drift instead of silently accepting it.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        WriteIndented = true,
        // Keep Cyrillic, emoji and quotes readable in failure messages.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Typed DTOs, anonymous objects and JsonNode are all serialized as is:
    /// negative tests rely on sending invalid data.
    /// </summary>
    public static string? Serialize(object? payload) =>
        payload is null ? null : JsonSerializer.Serialize(payload, payload.GetType(), Options);

    public static string Pretty(string json)
    {
        try
        {
            return JsonNode.Parse(json)?.ToJsonString(PrettyOptions) ?? "null";
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
