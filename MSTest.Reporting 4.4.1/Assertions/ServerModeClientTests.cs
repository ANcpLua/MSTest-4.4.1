using Microsoft.Testing.Platform.ServerMode.Client;
using MSTest.Showcase;

namespace MSTest.Reporting.Assertions;

/// <summary>
///     What a client of the server mode, such as the test explorer of an IDE, receives for failures of
///     <see cref="FailingAssertionTests" />. The node of a failed test carries the expected and the actual text of
///     the <c>AssertionFailureProperty</c> under the keys <c>assert.expected</c> and <c>assert.actual</c>, next to
///     <c>error.message</c>. The client of the package <c>Microsoft.Testing.Platform.ServerMode.Client.Sources</c>
///     starts this test application in server mode, discovers its tests and runs three of them.
/// </summary>
[TestClass]
public sealed class ServerModeClientTests
{
    private const string Expected = "assert.expected";
    private const string Actual = "assert.actual";

    private static readonly string[] Tests =
    [
        nameof(FailingAssertionTests.Compares_two_numbers),
        nameof(FailingAssertionTests.Looks_for_an_item),
        nameof(FailingAssertionTests.Fails_without_a_comparison)
    ];

    private static List<MtpTestNodeUpdate> _failures = null!;

    [ClassInitialize]
    public static async Task RunThreeFailingAssertionsInServerMode(TestContext testContext)
    {
        var options = new MtpServerClientOptions { EnvironmentVariables = { [ExplicitAttribute.Variable] = "1" } };
        using var client =
            await MtpServerClient.LaunchAsync(Environment.ProcessPath!, options, testContext.CancellationToken);
        var nodes = new List<MtpTestNodeUpdate>();
        client.TestNodesUpdated += (_, update) => nodes.AddRange(update.Changes);
        await client.InitializeAsync(testContext.CancellationToken);
        await client.DiscoverTestsAsync(testContext.CancellationToken);
        var uids = nodes.Where(static node => Tests.Contains(node.DisplayName)).Select(static node => node.Uid!)
            .ToList();
        nodes.Clear();

        await client.RunTestsAsync(uids, testContext.CancellationToken);

        _failures = [.. nodes.Where(static node => node.ExecutionState == "failed")];
        await client.ExitAsync(testContext.CancellationToken);
    }

    [TestMethod]
    public void Comparison_reaches_the_client_with_both_values()
    {
        var failure = FailureOf(nameof(FailingAssertionTests.Compares_two_numbers));

        Assert.AreEqual("15", failure.Node[Expected]);
        Assert.AreEqual("12.5", failure.Node[Actual]);
    }

    [TestMethod]
    public void Value_that_the_assertion_does_not_report_reaches_the_client_as_an_empty_text()
    {
        var failure = FailureOf(nameof(FailingAssertionTests.Looks_for_an_item));

        Assert.AreEqual("\"plum\"", failure.Node[Expected]);
        Assert.AreEqual(string.Empty, failure.Node[Actual]);
    }

    [TestMethod]
    public void Failure_without_a_comparison_reaches_the_client_with_empty_values_where_a_comparison_has_them()
    {
        var failure = FailureOf(nameof(FailingAssertionTests.Fails_without_a_comparison));

        Assert.AreEqual(string.Empty, failure.Node[Expected]);
        Assert.AreEqual(string.Empty, failure.Node[Actual]);
        Assert.Contains("The basket is not checked.", failure.ErrorMessage!);
        Assert.AreEqual("15", FailureOf(nameof(FailingAssertionTests.Compares_two_numbers)).Node[Expected]);
    }

    private static MtpTestNodeUpdate FailureOf(string test)
    {
        return Assert.ContainsSingle(_failures.Where(failure => failure.DisplayName == test));
    }
}