using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.Artifacts;

/// <summary>
/// With <c>SHOWCASE_OUTCOME_LOG=1</c>, records final test outcomes and publishes a session file artifact
/// with a producer-asserted kind. The composite factory in <c>Program.cs</c> shares one instance between
/// the data consumer and the session lifetime handler, so the handler writes the outcomes it consumed.
/// </summary>
internal sealed class OutcomeLog(IServiceProvider services) : IDataConsumer, ITestSessionLifetimeHandler, IDataProducer
{
    public const string Variable = "SHOWCASE_OUTCOME_LOG";
    public const string FileName = "outcomes.json";
    public const string Kind = "showcase.testing.outcomes";

    private readonly ConcurrentDictionary<string, Entry> _outcomes = new(StringComparer.Ordinal);

    public string Uid => nameof(OutcomeLog);
    public string Version => "1.0.0";
    public string DisplayName => "Outcome log";
    public string Description => "Writes the final outcome of each test as a session artifact.";
    public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];
    public Type[] DataTypesProduced => [typeof(SessionFileArtifact)];

    public Task<bool> IsEnabledAsync() => Task.FromResult(Environment.GetEnvironmentVariable(Variable) == "1");

    public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
    {
        var test = ((TestNodeUpdateMessage)value).TestNode;
        var state = test.Properties.SingleOrDefault<TestNodeStateProperty>() switch
        {
            null or DiscoveredTestNodeStateProperty or InProgressTestNodeStateProperty => null,
            PassedTestNodeStateProperty => "passed",
            FailedTestNodeStateProperty => "failed",
            ErrorTestNodeStateProperty => "error",
            TimeoutTestNodeStateProperty => "timeout",
            SkippedTestNodeStateProperty => "skipped",
            var unsupported => throw new NotSupportedException($"Unsupported test node state: {unsupported.GetType().FullName}")
        };

        if (state is null
            || test.Properties.SingleOrDefault<RetryAttemptProperty>() is { IsSuperseded: true })
        {
            return Task.CompletedTask;
        }

        _outcomes[test.Uid.Value] = new Entry(test.Uid.Value, test.DisplayName, state);
        return Task.CompletedTask;
    }

    public Task OnTestSessionStartingAsync(ITestSessionContext testSessionContext)
    {
        _outcomes.Clear();
        return Task.CompletedTask;
    }

    public async Task OnTestSessionFinishingAsync(ITestSessionContext testSessionContext)
    {
        var directory = services.GetConfiguration().GetTestResultDirectory();
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, FileName);
        await File.WriteAllTextAsync(path,
            JsonSerializer.Serialize(_outcomes.Values.OrderBy(static entry => entry.Uid, StringComparer.Ordinal)),
            testSessionContext.CancellationToken);

        await services.GetMessageBus().PublishAsync(this, new SessionFileArtifact(
            testSessionContext.SessionUid, new FileInfo(path), DisplayName, Description, Kind));
    }

    internal sealed record Entry(string Uid, string Name, string State);
}
