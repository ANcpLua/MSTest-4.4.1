using Microsoft.Testing.Platform.ServerMode.Client;
using MSTest.Platform.ServerMode.Catalogue;
using MSTest.Showcase;

namespace MSTest.Platform.ServerMode.Client;

/// <summary>
/// A client of the server mode receives the compared values of a failed assertion apart from its message:
/// the node of the failed test carries <c>assert.expected</c> and <c>assert.actual</c>, which an IDE shows as
/// a difference without parsing the message. The values come from the <c>AssertionFailureProperty</c> that
/// MSTest adds to a failed comparison. A failure that compares nothing has both keys with an empty value.
/// The client reads them from <c>MtpTestNodeUpdate.Node</c>, the node as it arrived.
/// </summary>
[TestClass]
public sealed class AssertionValueTests(TestContext testContext)
{
    private const string Expected = "assert.expected";
    private const string Actual = "assert.actual";

    [TestMethod]
    public async Task Failed_comparison_carries_the_expected_and_the_actual_value()
    {
        var node = await RunAsync(nameof(GrossPriceTests.Adds_the_tax_to_the_net_price));

        Assert.AreEqual("failed", node.ExecutionState);
        Assert.AreEqual("119", node.Node[Expected]);
        Assert.AreEqual("107.00", node.Node[Actual]);
    }

    [TestMethod]
    public async Task Failure_that_compares_nothing_carries_empty_values()
    {
        var node = await RunAsync(nameof(OutOfPrintTests.Prices_a_book_that_is_out_of_print));

        Assert.AreEqual("failed", node.ExecutionState);
        Assert.AreEqual(string.Empty, node.Node[Expected]);
        Assert.AreEqual(string.Empty, node.Node[Actual]);
    }

    private async Task<MtpTestNodeUpdate> RunAsync(string test)
    {
        var options = new MtpServerClientOptions { EnvironmentVariables = { [ExplicitAttribute.Variable] = "1" } };
        using var client = await MtpServerClient.LaunchAsync(Environment.ProcessPath!, options, testContext.CancellationToken);
        var nodes = new List<MtpTestNodeUpdate>();
        client.TestNodesUpdated += (_, update) => nodes.AddRange(update.Changes);
        await client.InitializeAsync(testContext.CancellationToken);
        await client.DiscoverTestsAsync(testContext.CancellationToken);
        var uid = nodes.Single(node => node.DisplayName == test).Uid!;

        await client.RunTestsAsync([uid], testContext.CancellationToken);
        await client.ExitAsync(testContext.CancellationToken);

        return nodes[^1];
    }
}
