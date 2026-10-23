namespace JsonPlaceholder.ApiTesting.Api;

/// <summary>
/// Base for resource clients: a new resource only declares its endpoints.
/// Methods return raw responses and contain no assertions, so negative tests can inspect 4xx/5xx.
/// </summary>
public abstract class ResourceApi(ApiClient client)
{
    protected ApiClient Client { get; } = client;

    protected Task<ApiResponse> RequestAsync(HttpMethod method, string path, object? payload = null) =>
        Client.SendAsync(method, path, ApiJson.Serialize(payload));
}
