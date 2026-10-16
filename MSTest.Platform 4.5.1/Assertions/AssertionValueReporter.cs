using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.OutputDevice;

namespace MSTest.Platform.Assertions;

/// <summary>
/// A reporter that prints the values of each failed assertion. It reads them from the
/// <see cref="AssertionFailureProperty"/> of the test node and not from the text of the failure:
/// <c>Expected</c> and <c>Actual</c> are the texts of the two values, and the one that the assertion does not
/// have is <see langword="null"/>. A failure without values has no such property. <c>Program.cs</c> registers
/// the reporter with <c>builder.TestHost.AddDataConsumer</c>.
/// </summary>
internal sealed class AssertionValueReporter(IOutputDevice outputDevice) : IDataConsumer, IOutputDeviceDataProducer
{
    public const string None = "none";

    public string Uid => nameof(AssertionValueReporter);

    public string Version => "1.0.0";

    public string DisplayName => "Assertion value reporter";

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

        var values = test.Properties.SingleOrDefault<AssertionFailureProperty>() is { } failure
            ? $"expected {failure.Expected ?? None}, actual {failure.Actual ?? None}"
            : "no values";

        await outputDevice.DisplayAsync(this, new TextOutputDeviceData($"Values of {test.DisplayName}: {values}"), cancellationToken);
    }
}
