using JsonPlaceholder.ApiTesting;
using JsonPlaceholder.ApiTesting.Api;
using JsonPlaceholder.ApiTesting.Models;

namespace JsonPlaceholder.ApiTests.Unit;

[Category("unit")]
public sealed class TransportTests
{
    [Test]
    public async Task Get_IsRetriedOnTransientErrors()
    {
        var handler = new StubHandler(503, 502, 200);
        using var client = CreateClient(handler, retries: 2);

        var response = await client.SendAsync(HttpMethod.Get, "/posts");

        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(handler.Requests, Has.Count.EqualTo(3));
    }

    [TestCase("POST")]
    [TestCase("PUT")]
    [TestCase("PATCH")]
    [TestCase("DELETE")]
    public async Task Writes_AreNotRetried(string method)
    {
        var handler = new StubHandler(503, 200);
        using var client = CreateClient(handler, retries: 2);

        var response = await client.SendAsync(new HttpMethod(method), "/posts/1", """{"title": "example"}""");

        Assert.That(response.StatusCode, Is.EqualTo(503));
        Assert.That(handler.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ClientError_IsNotRetried()
    {
        var handler = new StubHandler(400, 200);
        using var client = CreateClient(handler, retries: 2);

        var response = await client.SendAsync(HttpMethod.Get, "/posts");

        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(handler.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ExhaustedRetries_ReturnLastResponse()
    {
        var handler = new StubHandler(503);
        using var client = CreateClient(handler, retries: 1);

        var response = await client.SendAsync(HttpMethod.Get, "/posts");

        Assert.That(response.StatusCode, Is.EqualTo(503));
        Assert.That(handler.Requests, Has.Count.EqualTo(2));
    }

    [Test]
    public void RequestTimeout_IsEnforced()
    {
        var handler = new StubHandler { Delay = TimeSpan.FromSeconds(5) };
        using var client = CreateClient(handler, retries: 2, requestTimeoutSeconds: 0.05);

        // A write is used on purpose: it is never retried, so exactly one attempt must time out.
        Assert.That(
            () => client.SendAsync(HttpMethod.Post, "/posts", """{"title": "slow response"}"""),
            Throws.TypeOf<TimeoutException>());
        Assert.That(handler.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public void Get_GetsFreshTimeoutOnEveryAttempt()
    {
        var handler = new StubHandler { Delay = TimeSpan.FromSeconds(5) };
        using var client = CreateClient(handler, retries: 1, requestTimeoutSeconds: 0.05);

        Assert.That(() => client.SendAsync(HttpMethod.Get, "/posts"), Throws.TypeOf<TimeoutException>());
        Assert.That(handler.Requests, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task BaseUrlPathPrefix_IsPreserved()
    {
        var handler = new StubHandler();
        using var client = CreateClient(handler, baseUrl: "https://api.test/api/v1/");

        await client.SendAsync(HttpMethod.Get, "/posts");

        Assert.That(handler.Requests[0].Url.AbsolutePath, Is.EqualTo("/api/v1/posts"));
    }

    [Test]
    public async Task Payloads_ReachServerExactlyAsGiven()
    {
        var handler = new StubHandler();
        using var client = CreateClient(handler);
        var posts = new PostsApi(client);

        await posts.CreateAsync(new PostPayload(UserId: 1, Title: "Title", Body: "Body"));
        await posts.CreateAsync(new { userId = "wrong", title = false });
        await posts.CreateAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(handler.Requests[0].Body, Is.EqualTo("""{"userId":1,"title":"Title","body":"Body"}"""));
            Assert.That(handler.Requests[1].Body, Is.EqualTo("""{"userId":"wrong","title":false}"""));
            Assert.That(handler.Requests[2].Body, Is.Null);
        }
    }

    [Test]
    public async Task MalformedJson_IsSentAsIsAndNotRetried()
    {
        var handler = new StubHandler(500, 200);
        using var client = CreateClient(handler, retries: 2);

        var response = await new PostsApi(client).CreateRawAsync("{not a json");

        Assert.That(response.StatusCode, Is.EqualTo(500));
        Assert.That(handler.Requests, Has.Count.EqualTo(1));
        var request = handler.Requests[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(request.Url.AbsolutePath, Is.EqualTo("/posts"));
            Assert.That(request.ContentType, Is.EqualTo("application/json"));
            Assert.That(request.Body, Is.EqualTo("{not a json"));
        }
    }

    private static ApiClient CreateClient(
        StubHandler handler,
        string baseUrl = "https://api.test",
        int retries = 0,
        double requestTimeoutSeconds = 10) =>
        new(new ApiSettings(baseUrl, requestTimeoutSeconds: requestTimeoutSeconds, retries: retries), handler: handler);
}
