using System.Text.Json.Nodes;
using JsonPlaceholder.ApiTesting;
using JsonPlaceholder.ApiTesting.Api;
using JsonPlaceholder.ApiTesting.Models;

namespace JsonPlaceholder.ApiTests.Api;

[Category("full")]
[Category("posts")]
public sealed class PostsTests
{
    // A record is immutable, so one instance can be shared by all tests without leaking changes between them.
    private static readonly PostPayload Payload =
        new(UserId: 1, Title: "API automation test", Body: "A reusable test payload.");

    private readonly PostsApi _posts = new(ApiSession.Client);

    [Test]
    [Category("smoke")]
    public async Task GetPosts_ReturnsValidListWithUniqueIds()
    {
        var response = await _posts.ListAsync();

        ApiAssert.Status(response, 200);
        var posts = ApiAssert.ModelList<Post>(response);
        Assert.That(posts, Is.Not.Empty);
        Assert.That(posts.Select(post => post.Id), Is.Unique);
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    public async Task FilterPostsByUser_ReturnsOnlyThatUsersPosts(int userId)
    {
        var response = await _posts.ListAsync(userId);

        ApiAssert.Status(response, 200);
        var posts = ApiAssert.ModelList<Post>(response);
        Assert.That(posts, Is.Not.Empty);
        Assert.That(posts.Select(post => post.UserId), Is.All.EqualTo(userId));
    }

    [TestCaseSource(nameof(NewPosts))]
    public async Task CreatePost_EchoesSubmittedFields(string title, string body)
    {
        var payload = new PostPayload(UserId: 1, Title: title, Body: body);

        var response = await _posts.CreateAsync(payload);

        ApiAssert.Status(response, 201);
        AssertMatches(ApiAssert.Model<Post>(response), payload);
    }

    [TestCase(1, Category = "smoke")]
    [TestCase(50)]
    [TestCase(100)]
    public async Task UpdatePost_ReplacesAllFields(int postId)
    {
        var response = await _posts.UpdateAsync(postId, Payload);

        ApiAssert.Status(response, 200);
        var post = ApiAssert.Model<Post>(response);
        Assert.That(post.Id, Is.EqualTo(postId));
        AssertMatches(post, Payload);
    }

    [Test]
    public async Task PatchPost_ChangesOnlySubmittedField(
        [Values(1, 100)] int postId,
        [Values("""{"title": "Updated title only"}""", """{"body": "Updated body only"}""", """{"userId": 7}""")]
        string patchJson)
    {
        var original = await _posts.GetAsync(postId);
        ApiAssert.Status(original, 200);
        ApiAssert.Model<Post>(original);
        var patch = JsonNode.Parse(patchJson)!.AsObject();

        var response = await _posts.PatchAsync(postId, patch);

        ApiAssert.Status(response, 200);
        ApiAssert.Model<Post>(response);
        var expected = ApiAssert.Json(original).AsObject();
        foreach (var (field, value) in patch)
        {
            expected[field] = value?.DeepClone();
        }

        ApiAssert.JsonEquals(response, expected);
    }

    [TestCase(1)]
    [TestCase(100)]
    public async Task DeletePost_ReturnsEmptyObject(int postId)
    {
        var response = await _posts.DeleteAsync(postId);

        ApiAssert.Status(response, 200);
        ApiAssert.JsonEquals(response, "{}");
    }

    [Test]
    [Category("negative")]
    public async Task GetPost_WithMissingId_Returns404()
    {
        var response = await _posts.GetAsync(9999);

        ApiAssert.Status(response, 404);
        ApiAssert.JsonEquals(response, "{}");
    }

    private static IEnumerable<TestCaseData> NewPosts()
    {
        yield return new TestCaseData("Test title", "Test body").SetArgDisplayNames("basic").SetCategory("smoke");
        yield return new TestCaseData("Café 🚀", "Résumé — こんにちは 🌍").SetArgDisplayNames("unicode");
        yield return new TestCaseData(new string('x', 500), new string('y', 2000)).SetArgDisplayNames("long text");
        yield return new TestCaseData("<b>spec</b> & \"quotes\"", "line1\nline2\tend\\path")
            .SetArgDisplayNames("special characters");
    }

    private static void AssertMatches(Post post, PostPayload payload)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(post.UserId, Is.EqualTo(payload.UserId));
            Assert.That(post.Title, Is.EqualTo(payload.Title));
            Assert.That(post.Body, Is.EqualTo(payload.Body));
        }
    }
}
