using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.OutputDevice;

namespace MSTest.Reporting.Assertions;

/// <summary>
/// A reporter that prints one line for each failed test. It reads the expected and the actual value from the
/// <see cref="AssertionFailureProperty"/> of the test node and not from the failure message. A value that the
/// assertion does not report is <see langword="null"/>, and a failure without values has no such property.
/// <c>Program.cs</c> registers the reporter with <c>builder.TestHost.AddDataConsumer</c>.
/// </summary>
internal sealed class AssertionFailureReporter(IOutputDevice outputDevice) : IDataConsumer, IOutputDeviceDataProducer
{
    public const string NoValue = "(none)";

    public string Uid => nameof(AssertionFailureReporter);

    public string Version => "1.0.0";

    public string DisplayName => "Assertion failure reporter";

    public string Description => "Prints the expected and the actual value of each failed assertion.";

    public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public async Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
    {
        var test = ((TestNodeUpdateMessage)value).TestNode;

        if (test.Properties.SingleOrDefault<TestNodeStateProperty>() is not FailedTestNodeStateProperty)
        {
            return;
        }

        var line = test.Properties.SingleOrDefault<AssertionFailureProperty>() is { } failure
            ? $"{test.DisplayName} | expected: {failure.Expected ?? NoValue} | actual: {failure.Actual ?? NoValue}"
            : $"{test.DisplayName} | no values";

        await outputDevice.DisplayAsync(this, new TextOutputDeviceData(line), cancellationToken);
    }
}
