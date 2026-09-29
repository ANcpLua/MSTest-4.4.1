using Microsoft.Testing.Platform.ServerMode.Client;
using MSTest.Platform.ServerMode.Catalogue;
using MSTest.Showcase;

namespace MSTest.Platform.ServerMode.Client;

/// <summary>
/// The package <c>Microsoft.Testing.Platform.ServerMode.Client.Sources</c> adds the source of a client for
/// the server mode to the project, as internal types in the namespace
/// <c>Microsoft.Testing.Platform.ServerMode.Client</c>. <c>MtpServerClient.LaunchAsync</c> starts a test
/// application in server mode as a process of its own, here this test application once more.
/// <c>InitializeAsync</c> exchanges the capabilities, <c>DiscoverTestsAsync</c> and <c>RunTestsAsync</c>
/// report each test through <c>TestNodesUpdated</c> before they return, and <c>ExitAsync</c> asks the
/// application to end. <c>MtpServerClientOptions.EnvironmentVariables</c> adds variables to the environment of
/// the started process.
/// </summary>
[TestClass]
public sealed class ExternalProcessTests(TestContext testContext)
{
    private const string Pricing = nameof(PricingTests.Prices_a_book_with_its_discount);
    private const string OutOfPrint = nameof(OutOfPrintTests.Prices_a_book_that_is_out_of_print);

    [TestMethod]
    public async Task Server_answers_the_handshake_with_its_capabilities()
    {
        using var client = await MtpServerClient.LaunchAsync(Environment.ProcessPath!, cancellationToken: testContext.CancellationToken);

        var capabilities = await client.InitializeAsync(testContext.CancellationToken);

        Assert.IsTrue(capabilities.SupportsDiscovery);
        Assert.AreEqual("1.0.0", capabilities.ProtocolVersion);
        Assert.AreEqual("2.4.1", capabilities.ServerVersion);
        Assert.AreEqual(client.ProcessId, capabilities.ServerProcessId);
        Assert.AreNotEqual(Environment.ProcessId, client.ProcessId);

        await client.ExitAsync(testContext.CancellationToken);
    }

    [TestMethod]
    public async Task Discovery_reports_each_test_with_its_place_in_the_source()
    {
        using var client = await MtpServerClient.LaunchAsync(Environment.ProcessPath!, cancellationToken: testContext.CancellationToken);
        var nodes = new List<MtpTestNodeUpdate>();
        client.TestNodesUpdated += (_, update) => nodes.AddRange(update.Changes);
        await client.InitializeAsync(testContext.CancellationToken);

        await client.DiscoverTestsAsync(testContext.CancellationToken);

        var pricing = Assert.ContainsSingle(nodes.Where(static node => node.DisplayName == Pricing));
        Assert.AreEqual("action", pricing.NodeType);
        Assert.AreEqual("discovered", pricing.ExecutionState);
        Assert.EndsWith(Path.Combine("Catalogue", $"{nameof(PricingTests)}.cs"), pricing.FilePath);
        Assert.AreEqual(9, pricing.LineStart);

        await client.ExitAsync(testContext.CancellationToken);
    }

    [TestMethod]
    public async Task Run_of_a_node_reports_its_outcome()
    {
        using var client = await MtpServerClient.LaunchAsync(Environment.ProcessPath!, cancellationToken: testContext.CancellationToken);
        var nodes = new List<MtpTestNodeUpdate>();
        client.TestNodesUpdated += (_, update) => nodes.AddRange(update.Changes);
        await client.InitializeAsync(testContext.CancellationToken);
        await client.DiscoverTestsAsync(testContext.CancellationToken);
        var pricing = nodes.Single(static node => node.DisplayName == Pricing).Uid!;
        nodes.Clear();

        await client.RunTestsAsync([pricing], testContext.CancellationToken);

        Assert.AreEqual(Pricing, nodes[^1].DisplayName);
        Assert.AreEqual("passed", nodes[^1].ExecutionState);
        Assert.IsNotNull(nodes[^1].DurationInMilliseconds);

        await client.ExitAsync(testContext.CancellationToken);
    }

    [TestMethod]
    public async Task Failed_test_reports_its_message()
    {
        var options = new MtpServerClientOptions { EnvironmentVariables = { [ExplicitAttribute.Variable] = "1" } };
        using var client = await MtpServerClient.LaunchAsync(Environment.ProcessPath!, options, testContext.CancellationToken);
        var nodes = new List<MtpTestNodeUpdate>();
        client.TestNodesUpdated += (_, update) => nodes.AddRange(update.Changes);
        await client.InitializeAsync(testContext.CancellationToken);
        await client.DiscoverTestsAsync(testContext.CancellationToken);
        var outOfPrint = nodes.Single(static node => node.DisplayName == OutOfPrint).Uid!;

        await client.RunTestsAsync([outOfPrint], testContext.CancellationToken);

        Assert.AreEqual("failed", nodes[^1].ExecutionState);
        Assert.Contains("The book has no list price.", nodes[^1].ErrorMessage!);

        await client.ExitAsync(testContext.CancellationToken);
    }
}
