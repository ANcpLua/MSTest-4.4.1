using System.Text.Json.Nodes;

namespace MSTest.Platform.Changed.ServerMode;

/// <summary>
///     A server collects the updates of the tests for a moment and sends them to its client as one
///     <c>testing/testUpdates/tests</c> message. A message that has the outcome of a test leaves out the update
///     that the same test is in progress: the client hears of <see cref="PickingTests.Picks_the_item_at_once" />
///     once, when it has passed, and of <see cref="PickingTests.Waits_for_the_forklift" /> twice, because that
///     test is still running when its first update is sent.
/// </summary>
[TestClass]
public sealed class UpdateBatchRunTests
{
    private const string InProgress = "in-progress";
    private const string Passed = "passed";

    private static JsonArray[] _batches = [];

    [ClassInitialize]
    public static async Task RunThePicking(TestContext testContext)
    {
        await using var server = await ServerModeSession.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(PickingTests).FullName}");

        await server.RequestAsync(
            """
            {
                "jsonrpc": "2.0",
                "id": 1,
                "method": "initialize",
                "params": {
                    "processId": 1,
                    "clientInfo": { "name": "showcase", "version": "1.0.0" },
                    "capabilities": { "testing": { "debuggerProvider": false } }
                }
            }
            """,
            testContext.CancellationToken);
        await server.RequestAsync(
            """
            {
                "jsonrpc": "2.0",
                "id": 2,
                "method": "testing/runTests",
                "params": { "runId": "00000000-0000-0000-0000-000000000002" }
            }
            """,
            testContext.CancellationToken);

        _batches =
        [
            .. server.Notifications
                .Where(static message => (string?)message["method"] == "testing/testUpdates/tests")
                .Select(static message => message["params"]!["changes"])
                .OfType<JsonArray>()
        ];
    }

    [TestMethod]
    public void Test_that_ends_within_a_message_is_reported_with_its_outcome_only()
    {
        Assert.AreSequenceEqual([Passed], StatesOf(nameof(PickingTests.Picks_the_item_at_once)));
    }

    [TestMethod]
    public void Test_that_outlasts_a_message_is_reported_in_progress_and_with_its_outcome()
    {
        Assert.AreSequenceEqual([InProgress, Passed], StatesOf(nameof(PickingTests.Waits_for_the_forklift)));
    }

    [TestMethod]
    public void No_message_reports_a_test_both_in_progress_and_with_its_outcome()
    {
        foreach (var batch in _batches)
        {
            var nodes = batch.Select(static change => change!["node"]!).ToArray();
            var finished = nodes.Where(static node => (string?)node["execution-state"] != InProgress)
                .Select(static node => (string?)node["uid"]);
            var running = nodes.Where(static node => (string?)node["execution-state"] == InProgress)
                .Select(static node => (string?)node["uid"]);

            Assert.IsEmpty(running.Intersect(finished));
        }
    }

    private static IEnumerable<string?> StatesOf(string test)
    {
        return _batches
            .SelectMany(static batch => batch.Select(static change => change!["node"]!))
            .Where(node => (string?)node["display-name"] == test)
            .Select(static node => (string?)node["execution-state"]);
    }
}