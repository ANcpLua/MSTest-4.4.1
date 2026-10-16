using Microsoft.Testing.Platform.ServerMode.Client;
using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.ServerMode;

/// <summary>
/// A client of the server mode declares in the handshake whether it keeps the state of the test nodes for the
/// whole session, as the test explorer of an editor does, with <c>MtpServerClientOptions.IsStateful</c> of the
/// package <c>Microsoft.Testing.Platform.ServerMode.Client.Sources</c>. An extension in the test application
/// tells the three cases apart with <c>GetIsStateful</c>: <see langword="true"/>, <see langword="false"/> and
/// <see langword="null"/> for a client that declared nothing, which includes the clients that predate the
/// capability. <c>IsStateful</c> treats a client that declared nothing as stateless. Here
/// <see cref="ClientCapabilityReport"/> reports both values back to the client of a run of
/// <see cref="ShelfTests"/>.
/// </summary>
[TestClass]
public sealed class StatefulCapabilityTests(TestContext testContext)
{
    [TestMethod]
    [DataRow(true, "Declared stateful: True. IsStateful: True.")]
    [DataRow(false, "Declared stateful: False. IsStateful: False.")]
    [DataRow(null, "Declared stateful: nothing. IsStateful: False.")]
    public async Task Test_application_tells_what_the_client_declared(bool? declared, string reported)
    {
        var options = new MtpServerClientOptions { IsStateful = declared };
        using var client = await MtpServerClient.LaunchAsync(Environment.ProcessPath!, options, testContext.CancellationToken);
        var log = new List<string>();
        var nodes = new List<MtpTestNodeUpdate>();
        client.LogReceived += (_, message) => log.Add(message.Message);
        client.TestNodesUpdated += (_, update) => nodes.AddRange(update.Changes);
        await client.InitializeAsync(testContext.CancellationToken);
        await client.DiscoverTestsAsync(testContext.CancellationToken);
        var shelf = nodes.Single(static node => node.DisplayName == nameof(ShelfTests.Counts_the_boxes_on_the_shelf)).Uid!;

        await client.RunTestsAsync([shelf], testContext.CancellationToken);

        Assert.Contains(reported, log);

        await client.ExitAsync(testContext.CancellationToken);
    }
}
