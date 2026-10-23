using JsonPlaceholder.ApiTesting;

namespace JsonPlaceholder.ApiTests.Support;

/// <summary>
/// A strict "expected failure" for documented API deviations, see docs/KNOWN_LIMITATIONS.md.
/// While the issue reproduces, the test is reported as skipped with the issue ID.
/// Once the API is fixed, the test fails as a reminder to remove the wrapper.
/// </summary>
public static class KnownIssue
{
    public static void Expect(string id, string reason, Action check)
    {
        try
        {
            check();
        }
        catch (ApiAssertionException ex)
        {
            // This works only because ApiAssert throws its own exception: a failed NUnit assertion
            // is recorded in the test result before it is thrown, so catching it would not help.
            TestContext.Out.WriteLine(ex.Message);
            Assert.Ignore($"{id}: {reason}");
        }

        Assert.Fail($"{id} no longer reproduces ({reason}). Remove KnownIssue.Expect from this test.");
    }
}
