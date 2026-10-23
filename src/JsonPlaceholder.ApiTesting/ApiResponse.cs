namespace JsonPlaceholder.ApiTesting;

/// <summary>
/// A response with the body already read, so tests and failure messages can look at it as many times as needed.
/// </summary>
public sealed record ApiResponse(HttpMethod Method, Uri Url, int StatusCode, string? ContentType, string Body);
