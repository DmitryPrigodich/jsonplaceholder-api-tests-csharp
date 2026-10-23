using JsonPlaceholder.ApiTesting;
using JsonPlaceholder.ApiTesting.Models;

namespace JsonPlaceholder.ApiTests.Unit;

[Category("unit")]
public sealed class ContractTests
{
    [TestCase("\"1\"")]
    [TestCase("true")]
    [TestCase("1.5")]
    public void ResponseModel_DoesNotCoerceInvalidIdTypes(string id)
    {
        var response = JsonResponse(200, $$"""{"id": {{id}}, "userId": 1, "title": "Title", "body": "Body"}""");

        Assert.That(() => ApiAssert.Model<Post>(response), ThrowsSchemaError("Post"));
    }

    [Test]
    public void ResponseModel_RequiresApiFieldNames()
    {
        var response = JsonResponse(200, """{"id": 1, "user_id": 1, "title": "Title", "body": "Body"}""");

        Assert.That(() => ApiAssert.Model<Post>(response), ThrowsSchemaError("Post"));
    }

    [Test]
    public void ResponseModel_RejectsNullForRequiredText()
    {
        var response = JsonResponse(200, """{"id": 1, "userId": 1, "title": null, "body": "Body"}""");

        Assert.That(() => ApiAssert.Model<Post>(response), ThrowsSchemaError("Post"));
    }

    [Test]
    public void ResponseModel_RejectsNonPositiveId()
    {
        var response = JsonResponse(200, """[{"id": 0, "userId": 1, "title": "Title", "body": "Body"}]""");

        Assert.That(() => ApiAssert.ModelList<Post>(response), ThrowsSchemaError("Post at index 0"));
    }

    [Test]
    public void RequestDto_SerializesApiFieldNames()
    {
        var json = ApiJson.Serialize(new PostPayload(UserId: 1, Title: "Title", Body: "Body"));

        Assert.That(json, Is.EqualTo("""{"userId":1,"title":"Title","body":"Body"}"""));
    }

    [Test]
    public void StatusError_ShowsRequestAndResponse()
    {
        var error = Assert.Throws<ApiAssertionException>(() => ApiAssert.Status(JsonResponse(404, "{}"), 200));

        Assert.That(
            error.Message,
            Does.Contain("Expected HTTP 200")
                .And.Contain("GET https://example.com/posts")
                .And.Contain("HTTP 404"));
    }

    [Test]
    public void LongBody_IsTruncatedInFailureMessage()
    {
        var response = JsonResponse(500, $"\"{new string('x', 5000)}\"");

        var error = Assert.Throws<ApiAssertionException>(() => ApiAssert.Status(response, 200));

        Assert.That(error.Message, Does.Contain("more characters"));
        Assert.That(error.Message, Has.Length.LessThan(1500));
    }

    private static ApiResponse JsonResponse(int status, string body) =>
        new(HttpMethod.Get, new Uri("https://example.com/posts"), status, "application/json", body);

    private static NUnit.Framework.Constraints.IResolveConstraint ThrowsSchemaError(string what) =>
        Throws.TypeOf<ApiAssertionException>().With.Message.Contains($"Invalid {what} schema");
}
