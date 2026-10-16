using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Services;
using MSTest.Platform.Fixed.Controller;
using MSTest.Platform.Fixed.Orchestration;
using MSTest.Platform.Fixed.ServerMode;

namespace MSTest.Platform.Fixed;

/// <summary>
/// The entry point of the test application, written by hand because three extensions of the examples are
/// registered in code. <c>AddSelfRegisteredExtensions</c> registers MSTest and the extensions of the project,
/// as the generated entry point does. <see cref="ClientCapabilityReport"/> runs in the test host,
/// <see cref="TestHostStartupHooks"/> in the controller that starts the test host, and <see cref="RunLedger"/>
/// in the orchestrator that <c>--retry-failed-tests</c> starts.
/// </summary>
internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = await TestApplication.CreateBuilderAsync(args);

        builder.AddSelfRegisteredExtensions(args);
        builder.TestHost.AddTestSessionLifetimeHandler(static services => new ClientCapabilityReport(services));
        builder.TestHostControllers.AddEnvironmentVariableProvider(static _ => new TestHostStartupHooks());
        builder.TestHostOrchestrator.AddTestHostOrchestratorApplicationLifetime(static services => new RunLedger(services.GetOutputDevice()));

        using var application = await builder.BuildAsync();

        return await application.RunAsync();
    }
}
