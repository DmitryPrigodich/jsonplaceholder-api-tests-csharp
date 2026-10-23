using Microsoft.Extensions.Logging;

namespace JsonPlaceholder.ApiTests.Support;

/// <summary>
/// Writes framework logs into the output of the current test, so requests show up next to the failure in the report.
/// </summary>
internal sealed class NUnitLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            TestContext.Out.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {logLevel,-11} {formatter(state, exception)}");
        }
    }
}
