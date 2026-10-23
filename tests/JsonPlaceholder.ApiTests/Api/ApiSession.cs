using JsonPlaceholder.ApiTesting;
using JsonPlaceholder.ApiTests.Support;

namespace JsonPlaceholder.ApiTests.Api;

/// <summary>
/// One client for all API tests in this namespace. Unit tests live elsewhere and never touch the network.
/// </summary>
[SetUpFixture]
public sealed class ApiSession
{
    private static ApiClient? _client;

    public static ApiClient Client =>
        _client ?? throw new InvalidOperationException("ApiSession.Start has not run");

    [OneTimeSetUp]
    public void Start()
    {
        var settings = ApiSettings.Load(name => TestContext.Parameters.Get(name));
        TestContext.Progress.WriteLine(settings);
        _client = new ApiClient(settings, new NUnitLogger());
    }

    [OneTimeTearDown]
    public void Stop() => _client?.Dispose();
}
