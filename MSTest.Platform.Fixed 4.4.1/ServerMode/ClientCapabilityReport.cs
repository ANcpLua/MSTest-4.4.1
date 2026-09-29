using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.Fixed.ServerMode;

/// <summary>
/// An extension of the test application that reports what the client of a session in server mode declared
/// about the state of the test nodes. <c>GetIsStateful</c> on <c>IClientInfo.Capabilities</c> returns
/// <see langword="true"/> for a client that keeps the state, <see langword="false"/> for a client that declared
/// that it keeps none and <see langword="null"/> for a client that declared nothing. <c>IsStateful</c> is
/// <see langword="false"/> in both of the last two cases. In server mode the text that an extension displays
/// reaches the client as a log message.
/// </summary>
internal sealed class ClientCapabilityReport(IServiceProvider services) : ITestSessionLifetimeHandler, IOutputDeviceDataProducer
{
    public string Uid => nameof(ClientCapabilityReport);

    public string Version => "1.0.0";

    public string DisplayName => "Client capability report";

    public string Description => "Reports what the client of a session in server mode declared about the state of the nodes.";

    public Task<bool> IsEnabledAsync() => Task.FromResult(services.GetCommandLineOptions().IsOptionSet("server"));

    public Task OnTestSessionStartingAsync(ITestSessionContext testSessionContext)
    {
        var capabilities = services.GetClientInfo().Capabilities;
        var declared = capabilities.GetIsStateful()?.ToString() ?? "nothing";

        return services.GetOutputDevice().DisplayAsync(
            this,
            new TextOutputDeviceData($"Declared stateful: {declared}. IsStateful: {capabilities.IsStateful}."),
            testSessionContext.CancellationToken);
    }

    public Task OnTestSessionFinishingAsync(ITestSessionContext testSessionContext) => Task.CompletedTask;
}
