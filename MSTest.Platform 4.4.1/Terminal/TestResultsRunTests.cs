namespace MSTest.Platform.Terminal;

/// <summary>
/// <c>--show-test-results &lt;outcomes&gt;</c> selects the results that the terminal prints for a run of
/// <see cref="ShipmentTests"/>: <c>passed</c>, <c>failed</c>, <c>skipped</c>, <c>all</c> or <c>none</c>.
/// <c>failed</c> includes the test that exceeded its timeout. The three outcomes combine with a comma, with a
/// space or by repeating the option, and the selection takes precedence over <c>--output</c> wherever the two
/// stand. The summary counts every test, whatever the terminal prints.
/// </summary>
[TestClass]
public sealed class TestResultsRunTests(TestContext testContext)
{
    private const string Passed = $"passed {nameof(ShipmentTests.Ships_a_paid_order)}";
    private const string Failed = $"failed {nameof(ShipmentTests.Ships_an_order_that_costs_more_than_was_paid)}";
    private const string TimedOut = $"failed {nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)}";
    private const string Skipped = $"skipped {nameof(ShipmentTests.Ships_abroad)}";
    private const int AtLeastOneTestFailed = 2;

    [TestMethod]
    public async Task Default_shows_the_failed_and_the_skipped_results()
    {
        using var run = await RunTheShipmentsAsync();

        Assert.AreSequenceEqual([Failed, TimedOut, Skipped], run.ResultLines, SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public async Task Passed_shows_the_passed_results_only()
    {
        using var run = await RunTheShipmentsAsync("--show-test-results", "passed");

        Assert.AreSequenceEqual([Passed], run.ResultLines);
    }

    [TestMethod]
    public async Task Failed_includes_the_test_that_timed_out()
    {
        using var run = await RunTheShipmentsAsync("--show-test-results", "failed");

        Assert.AreSequenceEqual([Failed, TimedOut], run.ResultLines, SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public async Task Skipped_shows_the_skipped_results_only()
    {
        using var run = await RunTheShipmentsAsync("--show-test-results", "skipped");

        Assert.AreSequenceEqual([Skipped], run.ResultLines);
    }

    [TestMethod]
    public async Task All_shows_every_result()
    {
        using var run = await RunTheShipmentsAsync("--show-test-results", "all");

        Assert.AreSequenceEqual([Passed, Failed, TimedOut, Skipped], run.ResultLines, SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public async Task None_shows_no_result()
    {
        using var run = await RunTheShipmentsAsync("--show-test-results", "none");

        Assert.IsEmpty(run.ResultLines);
    }

    [TestMethod]
    [DataRow("--show-test-results", "passed,skipped")]
    [DataRow("--show-test-results", "passed", "skipped")]
    [DataRow("--show-test-results", "passed", "--show-test-results", "skipped")]
    public async Task Outcomes_combine_with_a_comma_a_space_or_a_repeated_option(params string[] selection)
    {
        using var run = await RunTheShipmentsAsync(selection);

        Assert.AreSequenceEqual([Passed, Skipped], run.ResultLines, SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    [DataRow("--output", "detailed", "--show-test-results", "skipped")]
    [DataRow("--show-test-results", "skipped", "--output", "detailed")]
    public async Task Selection_takes_precedence_over_the_output_option(params string[] options)
    {
        using var run = await RunTheShipmentsAsync(options);

        Assert.AreSequenceEqual([Skipped], run.ResultLines);
    }

    [TestMethod]
    public async Task Summary_and_exit_code_count_every_test_whatever_is_shown()
    {
        using var run = await RunTheShipmentsAsync("--show-test-results", "none");

        Assert.AreSequenceEqual(["total: 4", "failed: 2", "succeeded: 1", "skipped: 1"], run.LinesBelow("Test run summary:").Take(4));
        Assert.AreEqual(AtLeastOneTestFailed, run.ExitCode);
    }

    private Task<TestApplicationRun> RunTheShipmentsAsync(params string[] options) =>
        TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            ["--filter", $"ClassName={typeof(ShipmentTests).FullName}", .. options]);
}
