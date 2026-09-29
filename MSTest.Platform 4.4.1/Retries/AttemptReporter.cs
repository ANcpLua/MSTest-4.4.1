using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.OutputDevice;

namespace MSTest.Platform.Retries;

/// <summary>
/// A reporter that prints one line for each attempt of a test that its framework retried within the run.
/// The framework reports every attempt as an update of the same test node and adds a
/// <see cref="RetryAttemptProperty"/> to it: <c>AttemptNumber</c> counts from 1 and <c>IsSuperseded</c> is
/// <see langword="true"/> while another attempt follows, so the attempt that is not superseded has the
/// outcome of the test. A test that ran once has no such property. <c>Program.cs</c> registers the reporter
/// with <c>builder.TestHost.AddDataConsumer</c>.
/// </summary>
internal sealed class AttemptReporter(IOutputDevice outputDevice) : IDataConsumer, IOutputDeviceDataProducer
{
    public string Uid => nameof(AttemptReporter);

    public string Version => "1.0.0";

    public string DisplayName => "Attempt reporter";

    public string Description => "Prints the attempts of each retried test.";

    public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public async Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
    {
        var test = ((TestNodeUpdateMessage)value).TestNode;

        if (test.Properties.SingleOrDefault<RetryAttemptProperty>() is not { } attempt)
        {
            return;
        }

        var outcome = test.Properties.Single<TestNodeStateProperty>() is PassedTestNodeStateProperty ? "passed" : "failed";
        var rank = attempt.IsSuperseded ? "superseded" : "final";

        await outputDevice.DisplayAsync(
            this,
            new TextOutputDeviceData($"Attempt {attempt.AttemptNumber} of {test.DisplayName}: {outcome}, {rank}"),
            cancellationToken);
    }
}
