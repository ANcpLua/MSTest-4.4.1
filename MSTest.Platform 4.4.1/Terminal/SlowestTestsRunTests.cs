namespace MSTest.Platform.Terminal;

/// <summary>
/// <c>--show-slowest-tests &lt;count&gt;</c> adds the section <c>Slowest tests:</c> below the summary of the run.
/// It lists the given number of tests with their duration, the slowest first. The count is a whole number
/// from 1, and without the option the terminal prints no such section.
/// </summary>
[TestClass]
public sealed class SlowestTestsRunTests(TestContext testContext)
{
    private const string Heading = "Slowest tests:";
    private const int InvalidCommandLine = 5;

    private static readonly string[] StockCounts = ["--filter", $"ClassName={typeof(StockCountTests).FullName}"];

    [TestMethod]
    public async Task Section_lists_the_requested_number_of_tests_with_the_slowest_first()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, [.. StockCounts, "--show-slowest-tests", "2"]);

        var slowest = run.LinesBelow(Heading);

        Assert.HasCount(2, slowest);
        Assert.MatchesRegex($@"^\d+ms {nameof(StockCountTests.Counts_the_whole_warehouse)}$", slowest[0]);
        Assert.MatchesRegex($@"^\d+ms {nameof(StockCountTests.Counts_one_aisle)}$", slowest[1]);
        Assert.AreEqual(0, run.ExitCode);
    }

    [TestMethod]
    public async Task Section_is_missing_without_the_option()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, StockCounts);

        Assert.DoesNotContain(Heading, run.Output);
        Assert.AreEqual(0, run.ExitCode);
    }

    [TestMethod]
    [DataRow("0")]
    [DataRow("all")]
    public async Task Count_that_is_no_number_from_one_is_rejected(string count)
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, [.. StockCounts, "--show-slowest-tests", count]);

        Assert.Contains(
            "Option '--show-slowest-tests' has invalid arguments: --show-slowest-tests expects a single positive integer parameter.",
            run.Output);
        Assert.AreEqual(InvalidCommandLine, run.ExitCode);
    }
}
