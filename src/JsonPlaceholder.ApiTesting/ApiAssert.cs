using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonPlaceholder.ApiTesting;

/// <summary>
/// Reusable HTTP, JSON and schema checks. Every failure shows the request, the status and the response body.
/// </summary>
public static class ApiAssert
{
    private const int MaxBodyChars = 1000;

    public static void Status(ApiResponse response, params int[] expected)
    {
        if (!expected.Contains(response.StatusCode))
        {
            throw new ApiAssertionException($"Expected HTTP {string.Join(" or ", expected)}{Details(response)}");
        }
    }

    public static JsonNode Json(ApiResponse response)
    {
        if (!string.Equals(response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiAssertionException(
                $"Expected application/json, got '{response.ContentType}'{Details(response)}");
        }

        try
        {
            return JsonNode.Parse(response.Body)
                ?? throw new ApiAssertionException($"Expected a JSON value, got null{Details(response)}");
        }
        catch (JsonException ex)
        {
            throw new ApiAssertionException($"Invalid JSON{Details(response)}", ex);
        }
    }

    public static void JsonEquals(ApiResponse response, string expectedJson) =>
        JsonEquals(response, JsonNode.Parse(expectedJson)!);

    public static void JsonEquals(ApiResponse response, JsonNode expected)
    {
        if (!JsonNode.DeepEquals(Json(response), expected))
        {
            throw new ApiAssertionException(
                $"Expected body:\n{ApiJson.Pretty(expected.ToJsonString())}{Details(response)}");
        }
    }

    public static T Model<T>(ApiResponse response) => Deserialize<T>(Json(response), typeof(T).Name, response);

    public static List<T> ModelList<T>(ApiResponse response)
    {
        if (Json(response) is not JsonArray items)
        {
            throw new ApiAssertionException($"Expected a JSON array{Details(response)}");
        }

        return items
            .Select((item, index) => Deserialize<T>(item, $"{typeof(T).Name} at index {index}", response))
            .ToList();
    }

    private static T Deserialize<T>(JsonNode? json, string what, ApiResponse response)
    {
        T? model;
        try
        {
            model = json.Deserialize<T>(ApiJson.Options);
        }
        catch (JsonException ex)
        {
            throw new ApiAssertionException($"Invalid {what} schema: {ex.Message}{Details(response)}", ex);
        }

        if (model is null)
        {
            throw new ApiAssertionException($"Invalid {what} schema: null{Details(response)}");
        }

        // System.Text.Json checks types and required fields; value rules like "positive ID" live in attributes.
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model, new ValidationContext(model), errors, validateAllProperties: true))
        {
            var messages = string.Join("; ", errors.Select(error => error.ErrorMessage));
            throw new ApiAssertionException($"Invalid {what} schema: {messages}{Details(response)}");
        }

        return model;
    }

    private static string Details(ApiResponse response) =>
        $"\nRequest:  {response.Method} {response.Url}" +
        $"\nResponse: HTTP {response.StatusCode}" +
        $"\nBody:\n{FormatBody(response.Body)}";

    private static string FormatBody(string body)
    {
        if (body.Length == 0)
        {
            return "<empty body>";
        }

        var text = ApiJson.Pretty(body);
        return text.Length > MaxBodyChars
            ? $"{text[..MaxBodyChars]}\n... ({text.Length - MaxBodyChars} more characters)"
            : text;
    }
}
