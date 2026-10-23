using JsonPlaceholder.ApiTesting;

namespace JsonPlaceholder.ApiTests.Unit;

[Category("unit")]
public sealed class ApiSettingsTests
{
    [Test]
    public void RunParameters_OverrideEnvironment_AndEnvironmentOverridesDefaults()
    {
        var parameters = new Dictionary<string, string>
        {
            ["BaseUrl"] = "https://cli.example/v1/",
            ["Retries"] = "0",
        };
        var environment = new Dictionary<string, string>
        {
            ["API_BASE_URL"] = "https://environment.example/api/",
            ["API_REQUEST_TIMEOUT"] = "4.5",
            ["API_RETRIES"] = "1",
        };

        var settings = ApiSettings.Load(
            name => parameters.GetValueOrDefault(name),
            name => environment.GetValueOrDefault(name));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.BaseUrl, Is.EqualTo("https://cli.example/v1"));
            Assert.That(settings.RequestTimeout, Is.EqualTo(TimeSpan.FromSeconds(4.5)));
            Assert.That(settings.Retries, Is.Zero);
            Assert.That(settings.ConnectTimeout.TotalSeconds, Is.EqualTo(ApiSettings.DefaultConnectTimeoutSeconds));
        }
    }

    [TestCase("BaseUrl", "example.com")]
    [TestCase("BaseUrl", "ftp://example.com")]
    [TestCase("ConnectTimeout", "0")]
    [TestCase("RequestTimeout", "-1")]
    [TestCase("RequestTimeout", "fast")]
    [TestCase("Retries", "-1")]
    [TestCase("Retries", "6")]
    [TestCase("Retries", "two")]
    public void InvalidValue_IsRejected(string name, string value)
    {
        Assert.That(
            () => ApiSettings.Load(parameter => parameter == name ? value : null, _ => null),
            Throws.InstanceOf<ArgumentException>());
    }
}
