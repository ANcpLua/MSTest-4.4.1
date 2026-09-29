using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.ServerMode.Client;
using MSTest.Platform.ServerMode.Catalogue;

namespace MSTest.Platform.ServerMode.Client;

/// <summary>
/// <c>MtpServerClient.LaunchInProcessAsync</c> hosts a test application in the process of the client, for a
/// host that cannot start a process. The callback builds and runs the application with the arguments that the
/// client hands it, which make the application a server that connects to the client. <c>ShutdownAsync</c>
/// ends the client and the application without blocking the calling thread, and <c>ServerExitCode</c> is what
/// the callback returned, once the shutdown is over. <c>ServerShutdownTimeout</c> limits the wait for the
/// application. The class runs apart from the other classes, because it runs MSTest a second time in the
/// process.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class InProcessHostTests(TestContext testContext)
{
    private static readonly MtpServerClientOptions Options = new() { ServerShutdownTimeout = TimeSpan.FromSeconds(10) };

    private string[] _arguments = [];

    [TestMethod]
    public async Task Callback_receives_the_arguments_of_the_server_mode()
    {
        using var client = await MtpServerClient.LaunchInProcessAsync(RunTheTestApplicationAsync, Options, testContext.CancellationToken);

        await client.ShutdownAsync();

        Assert.AreSequenceEqual(["--server", "jsonrpc", "--client-host", "--client-port", "--no-banner"], _arguments.Where(static argument => argument.StartsWith('-') || argument == "jsonrpc"));
    }

    [TestMethod]
    public async Task Application_runs_in_the_process_of_the_client()
    {
        using var client = await MtpServerClient.LaunchInProcessAsync(RunTheTestApplicationAsync, Options, testContext.CancellationToken);

        var capabilities = await client.InitializeAsync(testContext.CancellationToken);

        Assert.AreEqual(Environment.ProcessId, capabilities.ServerProcessId);
        Assert.AreEqual(Environment.ProcessId, client.ProcessId);

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task Hosted_application_runs_a_test_and_returns_its_exit_code()
    {
        using var client = await MtpServerClient.LaunchInProcessAsync(RunTheTestApplicationAsync, Options, testContext.CancellationToken);
        var nodes = new List<MtpTestNodeUpdate>();
        client.TestNodesUpdated += (_, update) => nodes.AddRange(update.Changes);
        await client.InitializeAsync(testContext.CancellationToken);
        await client.DiscoverTestsAsync(testContext.CancellationToken);
        var pricing = nodes.Single(static node => node.DisplayName == nameof(PricingTests.Prices_a_book_with_its_discount)).Uid!;

        await client.RunTestsAsync([pricing], testContext.CancellationToken);
        await client.ExitAsync(testContext.CancellationToken);

        Assert.AreEqual("passed", nodes[^1].ExecutionState);
        Assert.IsNull(client.ServerExitCode);

        await client.ShutdownAsync();

        Assert.AreEqual(0, client.ServerExitCode);
    }

    private async Task<int> RunTheTestApplicationAsync(string[] arguments, CancellationToken cancellationToken)
    {
        _arguments = arguments;

        var builder = await TestApplication.CreateBuilderAsync(arguments);
        builder.AddMSTest(static () => [typeof(PricingTests).Assembly]);

        using var application = await builder.BuildAsync();

        return await application.RunAsync();
    }
}
