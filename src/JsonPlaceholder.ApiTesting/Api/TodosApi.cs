namespace JsonPlaceholder.ApiTesting.Api;

public sealed class TodosApi(ApiClient client) : ResourceApi(client)
{
    public Task<ApiResponse> ListAsync(int? userId = null, bool? completed = null)
    {
        var query = new List<string>();
        if (userId is not null)
        {
            query.Add($"userId={userId}");
        }

        if (completed is not null)
        {
            query.Add(completed.Value ? "completed=true" : "completed=false");
        }

        return RequestAsync(HttpMethod.Get, query.Count == 0 ? "/todos" : $"/todos?{string.Join('&', query)}");
    }

    public Task<ApiResponse> GetAsync(int todoId) => RequestAsync(HttpMethod.Get, $"/todos/{todoId}");
}
