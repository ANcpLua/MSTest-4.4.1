using Microsoft.Testing.Platform.ServerMode.Client;
using MSTest.Platform.ServerMode.Catalogue;

namespace MSTest.Platform.ServerMode.Client;

/// <summary>
/// <c>MtpServerClientOptions.IsStateful</c> declares in the handshake whether the client keeps the state of
/// the test nodes for the whole session. The client sends its name and its version as well. The test
/// application receives them as <c>IClientInfo</c>, which <see cref="ClientReport"/> reports back through
/// <c>LogReceived</c>. A client that declares nothing counts as one that keeps no state.
/// </summary>
[TestClass]
public sealed class StatefulClientTests(TestContext testContext)
{
    [TestMethod]
    [DataRow(true, "True")]
    [DataRow(false, "False")]
    [DataRow(null, "False")]
    public async Task Test_application_receives_what_the_client_declares(bool? declared, string received)
    {
        var options = new MtpServerClientOptions { ClientName = "showcase-explorer", ClientVersion = "4.4.1", IsStateful = declared };
        using var client = await MtpServerClient.LaunchAsync(Environment.ProcessPath!, options, testContext.CancellationToken);
        var nodes = new List<MtpTestNodeUpdate>();
        var log = new List<string>();
        client.TestNodesUpdated += (_, update) => nodes.AddRange(update.Changes);
        client.LogReceived += (_, message) => log.Add(message.Message);
        await client.InitializeAsync(testContext.CancellationToken);
        await client.DiscoverTestsAsync(testContext.CancellationToken);
        var pricing = nodes.Single(static node => node.DisplayName == nameof(PricingTests.Prices_a_book_with_its_discount)).Uid!;

        await client.RunTestsAsync([pricing], testContext.CancellationToken);

        Assert.Contains($"Client showcase-explorer 4.4.1 keeps the state of the nodes: {received}", log);

        await client.ExitAsync(testContext.CancellationToken);
    }
}
