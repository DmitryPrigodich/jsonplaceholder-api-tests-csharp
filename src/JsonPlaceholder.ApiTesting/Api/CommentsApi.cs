namespace JsonPlaceholder.ApiTesting.Api;

public sealed class CommentsApi(ApiClient client) : ResourceApi(client)
{
    public Task<ApiResponse> ForPostAsync(int postId) => RequestAsync(HttpMethod.Get, $"/posts/{postId}/comments");
}
