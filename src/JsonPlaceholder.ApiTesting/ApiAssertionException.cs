namespace JsonPlaceholder.ApiTesting;

/// <summary>
/// Thrown by <see cref="ApiAssert"/>. The library has its own exception so it does not depend on a test framework.
/// </summary>
public sealed class ApiAssertionException : Exception
{
    public ApiAssertionException(string message)
        : base(message)
    {
    }

    public ApiAssertionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
