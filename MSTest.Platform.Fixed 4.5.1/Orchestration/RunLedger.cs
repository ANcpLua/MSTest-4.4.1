using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Extensions.TestHostOrchestrator;
using Microsoft.Testing.Platform.OutputDevice;

namespace MSTest.Platform.Fixed.Orchestration;

/// <summary>
/// An extension of the orchestrator that opens an entry in a ledger before the orchestration and closes it
/// afterwards, as an extension does that creates a test run in a results service. The platform calls
/// <c>BeforeRunAsync</c> before the orchestrator starts a test host and <c>AfterRunAsync</c> with the exit
/// code once the orchestration is over, also when it failed or was cancelled. The token of
/// <c>AfterRunAsync</c> is already cancelled after a cancellation, so the release does not depend on it.
/// </summary>
internal sealed class RunLedger(IOutputDevice outputDevice) : ITestHostOrchestratorApplicationLifetime, IOutputDeviceDataProducer
{
    public string Uid => nameof(RunLedger);

    public string Version => "1.0.0";

    public string DisplayName => "Run ledger";

    public string Description => "Opens a ledger entry before the orchestration and closes it afterwards.";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task BeforeRunAsync(CancellationToken cancellationToken) =>
        outputDevice.DisplayAsync(this, new TextOutputDeviceData("Ledger entry opened."), cancellationToken);

    public Task AfterRunAsync(int exitCode, CancellationToken cancellationToken) =>
        outputDevice.DisplayAsync(this, new TextOutputDeviceData($"Ledger entry closed with exit code {exitCode}."), CancellationToken.None);
}
