using JsonPlaceholder.ApiTesting;
using JsonPlaceholder.ApiTesting.Api;
using JsonPlaceholder.ApiTesting.Models;

namespace JsonPlaceholder.ApiTests.Api;

[Category("full")]
[Category("todos")]
public sealed class TodosTests
{
    private readonly TodosApi _todos = new(ApiSession.Client);

    [Test]
    [Category("smoke")]
    public async Task GetTodos_ReturnsValidListWithUniqueIds()
    {
        var response = await _todos.ListAsync();

        ApiAssert.Status(response, 200);
        var todos = ApiAssert.ModelList<Todo>(response);
        Assert.That(todos, Is.Not.Empty);
        Assert.That(todos.Select(todo => todo.Id), Is.Unique);
    }

    [Test]
    public async Task FilterTodosByUserAndStatus_ReturnsOnlyMatchingTodos([Values] bool completed)
    {
        var response = await _todos.ListAsync(userId: 1, completed: completed);

        ApiAssert.Status(response, 200);
        var todos = ApiAssert.ModelList<Todo>(response);
        Assert.That(todos, Is.Not.Empty);
        Assert.That(todos, Has.All.Matches<Todo>(todo => todo.UserId == 1 && todo.Completed == completed));
    }

    [Test]
    public async Task GetTodoById_ReturnsThatTodo()
    {
        var response = await _todos.GetAsync(1);

        ApiAssert.Status(response, 200);
        Assert.That(ApiAssert.Model<Todo>(response).Id, Is.EqualTo(1));
    }

    [Test]
    [Category("negative")]
    public async Task GetTodo_WithMissingId_Returns404()
    {
        var response = await _todos.GetAsync(9999);

        ApiAssert.Status(response, 404);
        ApiAssert.JsonEquals(response, "{}");
    }
}
