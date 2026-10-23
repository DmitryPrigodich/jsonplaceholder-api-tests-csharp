namespace JsonPlaceholder.ApiTesting.Api;

public sealed class UsersApi(ApiClient client) : ResourceApi(client)
{
    public Task<ApiResponse> ListAsync() => RequestAsync(HttpMethod.Get, "/users");

    public Task<ApiResponse> GetAsync(int userId) => RequestAsync(HttpMethod.Get, $"/users/{userId}");

    /// <summary>For IDs that are not numbers at all, like "abc".</summary>
    public Task<ApiResponse> GetAsync(string userId) => RequestAsync(HttpMethod.Get, $"/users/{userId}");
}
