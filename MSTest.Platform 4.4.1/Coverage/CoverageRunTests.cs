using MSTest.Platform.Terminal;

namespace MSTest.Platform.Coverage;

/// <summary>
///     What the test host makes of the coverage that <see cref="FixedCoverageCollector" /> reports. The terminal
///     prints <c>Code Coverage Summary:</c> with the measurements of the whole run and of each module, one line
///     for each metric, and <c>Coverage Threshold Results:</c> with each check. A run whose tests pass ends with
///     exit code 14 when a check fails. A failed test keeps its own exit code.
///     <see cref="CoverageDigest" /> shows the same coverage as an extension reads it.
/// </summary>
[TestClass]
public sealed class CoverageRunTests(TestContext testContext)
{
    private const string Summary = "Code Coverage Summary:";
    private const string Thresholds = "Coverage Threshold Results:";
    private const int AtLeastOneTestFailed = 2;
    private const int CoverageThresholdFailed = 14;

    private static readonly string StockCounts = $"ClassName={typeof(StockCountTests).FullName}";
    private static readonly string Shipments = $"ClassName={typeof(ShipmentTests).FullName}";

    [TestMethod]
    public async Task Summary_lists_the_measurements_of_the_run_and_of_the_module()
    {
        using var run = await RunAsync(StockCounts, 170);

        Assert.AreSequenceEqual(
            ["Total - Line: 85.0%", "Total - Branch: 75.0%", $"{FixedCoverageCollector.Module} - Line: 85.0%"],
            run.LinesBelow(Summary));
    }

    [TestMethod]
    public async Task Check_that_is_met_is_listed_and_the_run_succeeds()
    {
        using var run = await RunAsync(StockCounts, 170);

        Assert.AreSequenceEqual(["Total - Line: 85.0% >= 80.0% threshold"], run.LinesBelow(Thresholds));
        Assert.AreEqual(0, run.ExitCode);
    }

    [TestMethod]
    public async Task Check_that_is_missed_ends_a_run_of_passed_tests_with_exit_code_14()
    {
        using var run = await RunAsync(StockCounts, 140);

        Assert.AreSequenceEqual(["Total - Line: 70.0% < 80.0% threshold"], run.LinesBelow(Thresholds));
        Assert.AreEqual(CoverageThresholdFailed, run.ExitCode);
    }

    [TestMethod]
    public async Task Failed_test_keeps_its_exit_code_when_a_check_is_missed_as_well()
    {
        using var run = await RunAsync(Shipments, 140);

        Assert.AreSequenceEqual(["Total - Line: 70.0% < 80.0% threshold"], run.LinesBelow(Thresholds));
        Assert.AreEqual(AtLeastOneTestFailed, run.ExitCode);
    }

    [TestMethod]
    public async Task Extension_reads_the_measurements_the_checks_and_the_report()
    {
        using var run = await RunAsync(StockCounts, 140);

        Assert.Contains(
            $"Coverage by {nameof(FixedCoverageCollector)}: 140 of 200 lines, 3 scopes, below the threshold, Lcov report {FixedCoverageCollector.Report}",
            run.Output);
        Assert.AreSequenceEqual(
            ["SF:Shop/Basket.cs", "LF:200", "LH:140", "end_of_record"],
            await File.ReadAllLinesAsync(run.PathOf(FixedCoverageCollector.Report), testContext.CancellationToken));
    }

    [TestMethod]
    public async Task Run_without_a_collector_prints_no_coverage()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, "--filter", StockCounts);

        Assert.DoesNotContain(Summary, run.Output);
        Assert.DoesNotContain(Thresholds, run.Output);
    }

    private Task<TestApplicationRun> RunAsync(string filter, int coveredLines)
    {
        return TestApplicationRun.StartAsync(
            new Dictionary<string, string> { [FixedCoverageCollector.Variable] = $"{coveredLines}" },
            testContext.CancellationToken,
            "--filter",
            filter);
    }
}