using JsonPlaceholder.ApiTesting;
using JsonPlaceholder.ApiTesting.Api;
using JsonPlaceholder.ApiTesting.Models;

namespace JsonPlaceholder.ApiTests.Api;

[Category("full")]
[Category("comments")]
public sealed class CommentsTests
{
    private readonly CommentsApi _comments = new(ApiSession.Client);

    [TestCase(1, Category = "smoke")]
    [TestCase(50)]
    [TestCase(100)]
    public async Task GetCommentsForPost_ReturnsOnlyThatPostsComments(int postId)
    {
        var response = await _comments.ForPostAsync(postId);

        ApiAssert.Status(response, 200);
        var comments = ApiAssert.ModelList<Comment>(response);
        Assert.That(comments, Is.Not.Empty);
        Assert.That(comments.Select(comment => comment.PostId), Is.All.EqualTo(postId));
        Assert.That(comments.Select(comment => comment.Id), Is.Unique);
    }

    [Test]
    [Category("negative")]
    public async Task GetCommentsForMissingPost_ReturnsEmptyList()
    {
        var response = await _comments.ForPostAsync(9999);

        ApiAssert.Status(response, 200);
        ApiAssert.JsonEquals(response, "[]");
    }
}
