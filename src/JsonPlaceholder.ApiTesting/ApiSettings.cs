using System.Globalization;

namespace JsonPlaceholder.ApiTesting;

/// <summary>
/// Test run settings. Priority: run parameters (.runsettings / CLI) > environment variables > defaults.
/// </summary>
public sealed class ApiSettings
{
    public const string DefaultBaseUrl = "https://jsonplaceholder.typicode.com";
    public const double DefaultConnectTimeoutSeconds = 3;
    public const double DefaultRequestTimeoutSeconds = 10;
    public const int DefaultRetries = 2;
    public const int MaxRetries = 5;

    private static readonly Dictionary<string, string> EnvironmentNames = new()
    {
        ["BaseUrl"] = "API_BASE_URL",
        ["ConnectTimeout"] = "API_CONNECT_TIMEOUT",
        ["RequestTimeout"] = "API_REQUEST_TIMEOUT",
        ["Retries"] = "API_RETRIES",
    };

    public ApiSettings(
        string baseUrl = DefaultBaseUrl,
        double connectTimeoutSeconds = DefaultConnectTimeoutSeconds,
        double requestTimeoutSeconds = DefaultRequestTimeoutSeconds,
        int retries = DefaultRetries)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException($"Base URL must start with http:// or https://, got '{baseUrl}'");
        }

        if (connectTimeoutSeconds <= 0)
        {
            throw new ArgumentException($"Connect timeout must be positive, got {connectTimeoutSeconds}");
        }

        if (requestTimeoutSeconds <= 0)
        {
            throw new ArgumentException($"Request timeout must be positive, got {requestTimeoutSeconds}");
        }

        if (retries is < 0 or > MaxRetries)
        {
            throw new ArgumentException($"Retries must be between 0 and {MaxRetries}, got {retries}");
        }

        BaseUrl = baseUrl.TrimEnd('/');
        ConnectTimeout = TimeSpan.FromSeconds(connectTimeoutSeconds);
        RequestTimeout = TimeSpan.FromSeconds(requestTimeoutSeconds);
        Retries = retries;
    }

    public string BaseUrl { get; }

    public TimeSpan ConnectTimeout { get; }

    /// <summary>Timeout of a single attempt, including reading the body.</summary>
    public TimeSpan RequestTimeout { get; }

    public int Retries { get; }

    public static ApiSettings Load(Func<string, string?> readParameter, Func<string, string?>? readEnvironment = null)
    {
        readEnvironment ??= Environment.GetEnvironmentVariable;

        string? Read(string name) =>
            NullIfBlank(readParameter(name)) ?? NullIfBlank(readEnvironment(EnvironmentNames[name]));

        return new ApiSettings(
            Read("BaseUrl") ?? DefaultBaseUrl,
            ParseDouble("ConnectTimeout", Read("ConnectTimeout")) ?? DefaultConnectTimeoutSeconds,
            ParseDouble("RequestTimeout", Read("RequestTimeout")) ?? DefaultRequestTimeoutSeconds,
            ParseInt("Retries", Read("Retries")) ?? DefaultRetries);
    }

    public override string ToString() =>
        $"API: {BaseUrl}; connect timeout: {ConnectTimeout.TotalSeconds}s; " +
        $"request timeout: {RequestTimeout.TotalSeconds}s; retries: {Retries}";

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static double? ParseDouble(string name, string? value)
    {
        if (value is null)
        {
            return null;
        }

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw new ArgumentException($"{name} must be a number of seconds, got '{value}'");
    }

    private static int? ParseInt(string name, string? value)
    {
        if (value is null)
        {
            return null;
        }

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw new ArgumentException($"{name} must be an integer, got '{value}'");
    }
}
