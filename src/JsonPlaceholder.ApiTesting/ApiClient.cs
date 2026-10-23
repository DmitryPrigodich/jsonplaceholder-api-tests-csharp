using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JsonPlaceholder.ApiTesting;

/// <summary>
/// One shared HttpClient with per-attempt timeouts, retries of GET requests and request logging.
/// </summary>
public sealed class ApiClient : IDisposable
{
    private static readonly HashSet<int> TransientStatuses = [429, 500, 502, 503, 504];
    private static readonly TimeSpan FirstRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly HttpClient _http;
    private readonly ILogger _logger;

    /// <param name="handler">Replaces the real network transport, for offline tests of the client itself.</param>
    public ApiClient(ApiSettings settings, ILogger? logger = null, HttpMessageHandler? handler = null)
    {
        Settings = settings;
        _logger = logger ?? NullLogger.Instance;

        // HttpClient.Timeout would cover all retries together, so the timeout is applied per attempt instead.
        _http = new HttpClient(handler ?? new SocketsHttpHandler { ConnectTimeout = settings.ConnectTimeout })
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public ApiSettings Settings { get; }

    public async Task<ApiResponse> SendAsync(
        HttpMethod method,
        string path,
        string? jsonBody = null,
        CancellationToken cancellationToken = default)
    {
        var url = new Uri($"{Settings.BaseUrl}/{path.TrimStart('/')}");
        // Only GET is retried: repeating a write could create the same resource twice.
        var retries = method == HttpMethod.Get ? Settings.Retries : 0;

        for (var attempt = 0; ; attempt++)
        {
            var isLastAttempt = attempt >= retries;
            try
            {
                var response = await SendOnceAsync(method, url, jsonBody, cancellationToken);
                if (isLastAttempt || !TransientStatuses.Contains(response.StatusCode))
                {
                    // No EnsureSuccessStatusCode(): negative tests need to inspect 4xx/5xx responses.
                    return response;
                }

                _logger.LogWarning("Retrying {Method} {Url} after HTTP {Status}", method, url, response.StatusCode);
            }
            catch (Exception ex) when (!isLastAttempt && ex is HttpRequestException or TimeoutException)
            {
                _logger.LogWarning("Retrying {Method} {Url} after error: {Error}", method, url, ex.Message);
            }

            await Task.Delay(FirstRetryDelay * Math.Pow(2, attempt), cancellationToken);
        }
    }

    public void Dispose() => _http.Dispose();

    private async Task<ApiResponse> SendOnceAsync(
        HttpMethod method,
        Uri url,
        string? jsonBody,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        if (jsonBody is not null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Settings.RequestTimeout);

        var started = Stopwatch.GetTimestamp();
        _logger.LogInformation("Request {Method} {Url}", method, url);
        try
        {
            using var response = await _http.SendAsync(request, timeout.Token);
            var body = await response.Content.ReadAsStringAsync(timeout.Token);
            var status = (int)response.StatusCode;
            _logger.LogInformation(
                "Response {Method} {Url} -> {Status} in {Seconds:F3}s",
                method,
                url,
                status,
                Stopwatch.GetElapsedTime(started).TotalSeconds);

            return new ApiResponse(method, url, status, response.Content.Headers.ContentType?.MediaType, body);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"{method} {url} got no response within {Settings.RequestTimeout.TotalSeconds}s");
        }
    }
}
