using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.ServerMode.Client;

/// <summary>
/// An extension of the test application that reports the client of a session in server mode.
/// <see cref="IClientInfo"/>, which <c>GetClientInfo</c> gets from the services of the platform, has the name
/// and the version of the client and its <c>Capabilities</c>. <c>IsStateful</c> tells whether the client
/// declared that it keeps the state of the test nodes for the whole session, as the test explorer of an
/// editor does. In server mode the text that an extension displays reaches the client as a log message.
/// </summary>
internal sealed class ClientReport(IServiceProvider services) : ITestSessionLifetimeHandler, IOutputDeviceDataProducer
{
    public string Uid => nameof(ClientReport);

    public string Version => "1.0.0";

    public string DisplayName => "Client report";

    public string Description => "Reports the client of a session in server mode.";

    public Task<bool> IsEnabledAsync() => Task.FromResult(services.GetCommandLineOptions().IsOptionSet("server"));

    public Task OnTestSessionStartingAsync(ITestSessionContext testSessionContext)
    {
        var client = services.GetClientInfo();

        return services.GetOutputDevice().DisplayAsync(
            this,
            new TextOutputDeviceData($"Client {client.Id} {client.Version} keeps the state of the nodes: {client.Capabilities.IsStateful}"),
            testSessionContext.CancellationToken);
    }

    public Task OnTestSessionFinishingAsync(ITestSessionContext testSessionContext) => Task.CompletedTask;
}
