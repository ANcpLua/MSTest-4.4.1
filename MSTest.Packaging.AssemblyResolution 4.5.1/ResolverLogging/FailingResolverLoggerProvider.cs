using Microsoft.Testing.Platform.Logging;

namespace MSTest.Packaging.AssemblyResolution.ResolverLogging;

/// <summary>
/// A logger provider whose loggers throw on every message of MSTest's assembly resolver and ignore all others.
/// </summary>
internal sealed class FailingResolverLoggerProvider : ILoggerProvider
{
    public static int FailureCount => Volatile.Read(ref _failureCount);
    private static int _failureCount;

    public ILogger CreateLogger(string categoryName) => new FailingResolverLogger();

    private sealed class FailingResolverLogger : ILogger
    {
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!formatter(state, exception).StartsWith("MSTest.AssemblyResolver.", StringComparison.Ordinal)) return;
            Interlocked.Increment(ref _failureCount);
            throw new InvalidOperationException("The logger fails on resolver messages.");
        }

        public Task LogAsync<TState>(LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Log(logLevel, state, exception, formatter);

            return Task.CompletedTask;
        }
    }
}
