using System.Text.Json;
using MSTest.Platform.Retries;
using MSTest.Platform.Terminal;

namespace MSTest.Platform.GitHubActions;

/// <summary>
///     <c>--report-gh-history &lt;path&gt;</c> keeps a snapshot of the results of the runs of GitHub Actions in a
///     JSON file. Each run adds one sample for each test with the id of the run from <c>GITHUB_RUN_ID</c>, the
///     outcome and whether the test passed only after a retry. The next run reads the snapshot and adds the
///     history of a failed test to its <c>::error</c> annotation. The workflow keeps the file between runs, for
///     example as an artifact that it downloads before and uploads after the tests. The runner variables that
///     this example sets itself include <c>RUNNER_OS</c>: a snapshot whose samples have no runner system is
///     rejected when the next run reads it.
/// </summary>
[TestClass]
public sealed class HistoryRunTests(TestContext testContext)
{
    private const string History = "history.json";

    [TestMethod]
    public async Task Snapshot_has_a_sample_for_each_test_of_the_run()
    {
        using var run = await RunAsync(Directory.CreateTempSubdirectory(), "101", typeof(ShipmentTests));

        var samples = SamplesOf(run.PathOf(History));

        Assert.AreSequenceEqual(
            [
                $"101 {nameof(ShipmentTests.Ships_a_paid_order)}: passed",
                $"101 {nameof(ShipmentTests.Ships_abroad)}: skipped",
                $"101 {nameof(ShipmentTests.Ships_an_order_that_costs_more_than_was_paid)}: failed",
                $"101 {nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)}: failed"
            ],
            samples.Select(static sample =>
                $"{sample.GetProperty("runId")} {sample.GetProperty("displayName")}: {sample.GetProperty("outcome")}"),
            SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public async Task Next_run_adds_the_history_to_the_annotation_of_a_failed_test()
    {
        using var first = await RunAsync(Directory.CreateTempSubdirectory(), "101", typeof(ShipmentTests));
        var second = await RunAsync(first.Results, "102", typeof(ShipmentTests));

        Assert.Contains(
            "timed out after 100ms Historical context: failed 1 and flaked 0 of 1 prior runs within the 30-day history window.",
            second.Output);
        Assert.DoesNotContain("Historical context", first.Output);
        Assert.HasCount(8, SamplesOf(first.PathOf(History)));
    }

    [TestMethod]
    public async Task Snapshot_without_the_runner_system_is_not_read()
    {
        using var first = await RunAsync(Directory.CreateTempSubdirectory(), "101", typeof(ShipmentTests), null);
        var second = await RunAsync(first.Results, "102", typeof(ShipmentTests), null);

        Assert.DoesNotContain("Historical context", second.Output);
        Assert.HasCount(4, SamplesOf(first.PathOf(History)));
    }

    [TestMethod]
    public async Task Sample_of_a_test_that_passes_after_a_retry_is_flaky()
    {
        using var run = await RunAsync(Directory.CreateTempSubdirectory(), "103", typeof(UnsteadyTests));

        var third = SamplesOf(run.PathOf(History))
            .Single(static sample => sample.GetProperty("displayName").GetString() ==
                                     nameof(UnsteadyTests.Scans_a_label_at_the_third_attempt));

        Assert.AreEqual("passed", third.GetProperty("outcome").GetString());
        Assert.IsTrue(third.GetProperty("isFlaky").GetBoolean());
    }

    private Task<TestApplicationRun> RunAsync(DirectoryInfo results, string runId, Type tests,
        string? runnerSystem = "macOS")
    {
        var runner = new Dictionary<string, string?>
        {
            ["GITHUB_ACTIONS"] = "true",
            ["GITHUB_RUN_ID"] = runId,
            ["GITHUB_STEP_SUMMARY"] = Path.Combine(results.FullName, $"summary-{runId}.md"),
            ["RUNNER_OS"] = runnerSystem
        };

        return TestApplicationRun.StartAsync(
            results,
            runner,
            testContext.CancellationToken,
            "--filter", $"ClassName={tests.FullName}",
            "--report-gh",
            "--report-gh-history", Path.Combine(results.FullName, History));
    }

    private static JsonElement[] SamplesOf(string history)
    {
        using var snapshot = JsonDocument.Parse(File.ReadAllText(history));

        return
            [.. snapshot.RootElement.GetProperty("samples").EnumerateArray().Select(static sample => sample.Clone())];
    }
}