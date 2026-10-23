namespace JsonPlaceholder.ApiTesting.Api;

public sealed class PostsApi(ApiClient client) : ResourceApi(client)
{
    public Task<ApiResponse> ListAsync(int? userId = null) =>
        RequestAsync(HttpMethod.Get, userId is null ? "/posts" : $"/posts?userId={userId}");

    public Task<ApiResponse> GetAsync(int postId) => RequestAsync(HttpMethod.Get, $"/posts/{postId}");

    public Task<ApiResponse> CreateAsync(object? payload = null) => RequestAsync(HttpMethod.Post, "/posts", payload);

    /// <summary>Sends the body exactly as given, including malformed JSON.</summary>
    public Task<ApiResponse> CreateRawAsync(string body) => Client.SendAsync(HttpMethod.Post, "/posts", body);

    public Task<ApiResponse> UpdateAsync(int postId, object payload) =>
        RequestAsync(HttpMethod.Put, $"/posts/{postId}", payload);

    public Task<ApiResponse> PatchAsync(int postId, object payload) =>
        RequestAsync(HttpMethod.Patch, $"/posts/{postId}", payload);

    public Task<ApiResponse> DeleteAsync(int postId) => RequestAsync(HttpMethod.Delete, $"/posts/{postId}");
}
