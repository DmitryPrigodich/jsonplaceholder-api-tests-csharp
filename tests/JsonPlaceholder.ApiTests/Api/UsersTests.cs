using JsonPlaceholder.ApiTesting;
using JsonPlaceholder.ApiTesting.Api;
using JsonPlaceholder.ApiTesting.Models;

namespace JsonPlaceholder.ApiTests.Api;

[Category("full")]
[Category("users")]
public sealed class UsersTests
{
    private readonly UsersApi _users = new(ApiSession.Client);

    [Test]
    [Category("smoke")]
    public async Task GetUsers_ReturnsValidListWithUniqueIds()
    {
        var response = await _users.ListAsync();

        ApiAssert.Status(response, 200);
        var users = ApiAssert.ModelList<User>(response);
        Assert.That(users, Is.Not.Empty);
        Assert.That(users.Select(user => user.Id), Is.Unique);
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    public async Task GetUserById_ReturnsThatUser(int userId)
    {
        var response = await _users.GetAsync(userId);

        ApiAssert.Status(response, 200);
        Assert.That(ApiAssert.Model<User>(response).Id, Is.EqualTo(userId));
    }

    [Category("negative")]
    [TestCase("9999", Category = "smoke")]
    [TestCase("0")]
    [TestCase("-1")]
    [TestCase("abc")]
    public async Task GetUser_WithMissingOrInvalidId_Returns404(string userId)
    {
        var response = await _users.GetAsync(userId);

        ApiAssert.Status(response, 404);
        ApiAssert.JsonEquals(response, "{}");
    }
}
