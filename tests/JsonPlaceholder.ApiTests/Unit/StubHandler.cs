using System.Net;
using System.Text;

namespace JsonPlaceholder.ApiTests.Unit;

/// <summary>
/// Fake transport: records what the client sends and answers with queued statuses. No network involved.
/// </summary>
internal sealed class StubHandler(params int[] statuses) : HttpMessageHandler
{
    // The last status repeats, so a single 503 means "always fails".
    private readonly Queue<int> _statuses = new(statuses.Length == 0 ? [200] : statuses);

    public TimeSpan Delay { get; init; }

    public List<RecordedRequest> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add(new RecordedRequest(
            request.Method,
            request.RequestUri!,
            request.Content?.Headers.ContentType?.MediaType,
            body));

        var status = _statuses.Count > 1 ? _statuses.Dequeue() : _statuses.Peek();
        await Task.Delay(Delay, cancellationToken);

        return new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent("""{"ok": true}""", Encoding.UTF8, "application/json"),
        };
    }
}

internal sealed record RecordedRequest(HttpMethod Method, Uri Url, string? ContentType, string? Body);
