using System.Text.Json.Nodes;
using JsonPlaceholder.ApiTesting;
using JsonPlaceholder.ApiTesting.Api;
using JsonPlaceholder.ApiTests.Support;

namespace JsonPlaceholder.ApiTests.Api;

/// <summary>
/// Assumed validation rules for POST /posts. JSONPlaceholder does not validate input, see docs/KNOWN_LIMITATIONS.md.
/// </summary>
[Category("full")]
[Category("negative")]
[Category("contract")]
[Category("posts")]
public sealed class InvalidPostPayloadTests
{
    private readonly PostsApi _posts = new(ApiSession.Client);

    [TestCaseSource(nameof(InvalidPayloads))]
    public async Task CreatePost_WithInvalidPayload_IsRejected(string? json)
    {
        var response = await _posts.CreateAsync(json is null ? null : JsonNode.Parse(json));

        KnownIssue.Expect(
            "JP-001",
            "JSONPlaceholder accepts invalid payloads with HTTP 201",
            () => ApiAssert.Status(response, 400, 422));
    }

    [Test]
    public async Task CreatePost_WithMalformedJson_IsRejected()
    {
        var response = await _posts.CreateRawAsync("{not a json");

        KnownIssue.Expect("JP-004", "malformed JSON causes HTTP 500", () => ApiAssert.Status(response, 400));
    }

    private static IEnumerable<TestCaseData> InvalidPayloads()
    {
        yield return Case("missing title", """{"body": "Body", "userId": 1}""");
        yield return Case("missing body", """{"title": "Title", "userId": 1}""");
        yield return Case("missing userId", """{"title": "Title", "body": "Body"}""");
        yield return Case("wrong title type", """{"title": 123, "body": "Body", "userId": 1}""");
        yield return Case("wrong body type", """{"title": "Title", "body": false, "userId": 1}""");
        yield return Case("wrong userId type", """{"title": "Title", "body": "Body", "userId": "wrong"}""");
        yield return Case("empty title", """{"title": "", "body": "Body", "userId": 1}""");
        yield return Case("empty body", """{"title": "Title", "body": "", "userId": 1}""");
        yield return Case("zero userId", """{"title": "Title", "body": "Body", "userId": 0}""");
        yield return Case("negative userId", """{"title": "Title", "body": "Body", "userId": -1}""");
        yield return Case("empty object", "{}");
        yield return Case("no request body", null);
    }

    private static TestCaseData Case(string name, string? json) => new TestCaseData(json).SetArgDisplayNames(name);
}
