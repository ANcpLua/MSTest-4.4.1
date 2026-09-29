using System.Runtime.Loader;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.Extension;

/// <summary>
/// An extension that prints a line when the test session starts and when it ends. The first line names the
/// load context of the assembly, which is a context of its own when a manifest loaded the extension.
/// </summary>
internal sealed class ShiftLog(IOutputDevice outputDevice, string shift) : ITestSessionLifetimeHandler, IOutputDeviceDataProducer
{
    public string Uid => $"{nameof(ShiftLog)}.{shift}";

    public string Version => "1.0.0";

    public string DisplayName => $"{shift} log";

    public string Description => "Prints the start and the end of the test session.";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task OnTestSessionStartingAsync(ITestSessionContext testSessionContext) =>
        outputDevice.DisplayAsync(
            this,
            new TextOutputDeviceData($"{shift} starts in the load context {AssemblyLoadContext.GetLoadContext(typeof(ShiftLog).Assembly)!.Name}"),
            testSessionContext.CancellationToken);

    public Task OnTestSessionFinishingAsync(ITestSessionContext testSessionContext) =>
        outputDevice.DisplayAsync(this, new TextOutputDeviceData($"{shift} ends"), testSessionContext.CancellationToken);
}
