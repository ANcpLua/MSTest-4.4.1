using System.Runtime.CompilerServices;
using MSTest.Platform.Coverage;
using MSTest.Platform.Terminal;

namespace MSTest.Platform.GitHubActions;

/// <summary>
///     The job summary that the GitHub Actions report writes to the file that <c>GITHUB_STEP_SUMMARY</c> names.
///     Below <c>### ❌ Failures</c> each failed test is a collapsible <c>&lt;details&gt;</c> block with its
///     exception type, its location and its message. The block is bounded: a message of more than thirty lines,
///     such as that of <see cref="ManifestTests" />, keeps its first thirty and ends with <c>[... truncated]</c>.
///     <c>--report-gh-failure-details off</c> lists the failed tests without these blocks.
///     <c>--report-gh-step-summary on-failure</c> writes the summary only when the run
///     does not end with exit code 0. <c>--report-gh-step-summary-sections</c> selects the sections:
///     <c>test-results</c>, <c>slow-tests</c>, <c>coverage</c> or <c>all</c>, and rejects any other value. The
///     report is active with <c>--report-gh</c> and the variable <c>GITHUB_ACTIONS</c> set to <c>true</c>. This
///     example sets the variables itself, as a runner of GitHub Actions does, with the root of this repository as
///     <c>GITHUB_WORKSPACE</c>.
/// </summary>
[TestClass]
public sealed class StepSummaryRunTests(TestContext testContext)
{
    private const string Summary = "summary.md";
    private const string Failures = "### ❌ Failures (2)";
    private const string Counts = "| Total | Passed | Failed | Skipped | Flaky | Duration |";
    private const string CoverageSection = "### Code coverage";
    private const string SlowestTests = "### ⏱ Slowest tests";
    private const int InvalidCommandLine = 5;

    private static readonly string Shipments = $"ClassName={typeof(ShipmentTests).FullName}";
    private static readonly string StockCounts = $"ClassName={typeof(StockCountTests).FullName}";

    [TestMethod]
    public async Task Each_failed_test_is_a_block_with_its_exception_location_and_message()
    {
        var summary = await SummarizeAsync(Shipments);

        Assert.Contains(Failures, summary);
        Assert.Contains(
            $"<details>\n<summary><code>MSTest.Platform.Terminal.ShipmentTests.{nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)}</code> — ",
            summary);
        Assert.Contains("**Exception:** `Microsoft.VisualStudio.TestTools.UnitTesting.MSTestTestNodeException`",
            summary);
        Assert.Contains("**Location:** `MSTest.Platform 4.4.1/Terminal/ShipmentTests.cs:20`", summary);
        Assert.Contains("**Location:** `MSTest.Platform 4.4.1/Terminal/ShipmentTests.cs:22`", summary);
        Assert.Contains(
            $"```text\nTest '{nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)}' timed out after 100ms\n```",
            summary);
    }

    [TestMethod]
    public async Task Long_message_is_clipped_to_thirty_lines()
    {
        var summary = await SummarizeAsync($"ClassName={typeof(ManifestTests).FullName}");

        Assert.Contains("Parcel 29 is missing.\n[... truncated]\n", summary);
        Assert.DoesNotContain("Parcel 30 is missing.", summary);
    }

    [TestMethod]
    public async Task Failure_details_that_are_off_list_the_failed_tests_only()
    {
        var summary = await SummarizeAsync(Shipments, "--report-gh-failure-details", "off");

        Assert.Contains(Failures, summary);
        Assert.Contains(
            $"- `MSTest.Platform.Terminal.ShipmentTests.{nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)}` — ",
            summary);
        Assert.DoesNotContain("<details>", summary);
    }

    [TestMethod]
    public async Task On_failure_writes_no_summary_for_a_run_that_succeeds()
    {
        using var run = await RunAsync(new Dictionary<string, string>(), StockCounts, "--report-gh-step-summary",
            "on-failure");

        Assert.AreEqual(0, run.ExitCode);
        Assert.IsFalse(File.Exists(run.PathOf(Summary)));
    }

    [TestMethod]
    public async Task On_failure_writes_the_summary_of_a_run_that_fails()
    {
        using var run = await RunAsync(new Dictionary<string, string>(), Shipments, "--report-gh-step-summary",
            "on-failure");

        Assert.AreEqual(2, run.ExitCode);
        Assert.Contains("## ❌ Test Run Summary — MSTest.Platform 4.4.1 (net10.0)",
            await File.ReadAllTextAsync(run.PathOf(Summary), testContext.CancellationToken));
    }

    [TestMethod]
    public async Task On_failure_follows_the_exit_code_and_not_only_the_tests()
    {
        using var run = await RunAsync(
            new Dictionary<string, string> { [FixedCoverageCollector.Variable] = "140" },
            StockCounts,
            "--report-gh-step-summary", "on-failure");

        Assert.AreEqual(14, run.ExitCode);
        Assert.Contains("| 3 | 3 | 0 | 0 | 0 |", await File.ReadAllTextAsync(run.PathOf(Summary), testContext.CancellationToken));
    }

    [TestMethod]
    public async Task Coverage_section_alone_leaves_out_the_counts_the_failures_and_the_slowest_tests()
    {
        var summary = await SummarizeAsync(
            new Dictionary<string, string> { [FixedCoverageCollector.Variable] = "140" },
            Shipments,
            "--report-gh-step-summary-sections", "coverage");

        Assert.Contains(CoverageSection, summary);
        Assert.DoesNotContain(Counts, summary);
        Assert.DoesNotContain(Failures, summary);
        Assert.DoesNotContain(SlowestTests, summary);
    }

    [TestMethod]
    public async Task Sections_combine_with_a_comma()
    {
        var summary = await SummarizeAsync(
            new Dictionary<string, string> { [FixedCoverageCollector.Variable] = "140" },
            Shipments,
            "--report-gh-step-summary-sections", "test-results,slow-tests");

        Assert.Contains(Counts, summary);
        Assert.Contains(Failures, summary);
        Assert.Contains(SlowestTests, summary);
        Assert.DoesNotContain(CoverageSection, summary);
    }

    [TestMethod]
    public async Task Unknown_section_is_rejected()
    {
        using var run = await RunAsync(new Dictionary<string, string>(), Shipments, "--report-gh-step-summary-sections",
            "coverage,flaky");

        Assert.Contains(
            "Invalid GitHub Actions job-summary section 'flaky'. Valid values are 'test-results', 'slow-tests', 'coverage', or 'all'.",
            run.Output);
        Assert.AreEqual(InvalidCommandLine, run.ExitCode);
    }

    private Task<string> SummarizeAsync(string filter, params string[] options)
    {
        return SummarizeAsync(new Dictionary<string, string>(), filter, options);
    }

    private async Task<string> SummarizeAsync(Dictionary<string, string> environment, string filter,
        params string[] options)
    {
        using var run = await RunAsync(environment, filter, options);

        return await File.ReadAllTextAsync(run.PathOf(Summary), testContext.CancellationToken);
    }

    private Task<TestApplicationRun> RunAsync(Dictionary<string, string> environment, string filter,
        params string[] options)
    {
        var results = Directory.CreateTempSubdirectory();
        environment["GITHUB_ACTIONS"] = "true";
        environment["GITHUB_STEP_SUMMARY"] = Path.Combine(results.FullName, Summary);
        environment["GITHUB_WORKSPACE"] = RepositoryRoot();

        return TestApplicationRun.StartAsync(results, environment, testContext.CancellationToken,
            ["--filter", filter, "--report-gh", .. options]);
    }

    private static string RepositoryRoot([CallerFilePath] string file = "")
    {
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, "..", ".."));
    }
}