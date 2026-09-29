namespace MSTest.Platform.Changed.GitHubSummary;

/// <summary>
/// The job summary of the GitHub Actions report has a section for the test assembly: a heading with the
/// verdict of the run, the name of the assembly and its target framework, and a table with the counts. The
/// verdict follows the exit code and not only the failed tests: a run whose tests all pass and that ends with
/// another exit code than 0 has a failed heading and a warning with the exit code, its name and its reason.
/// The summary is written to the file that <c>GITHUB_STEP_SUMMARY</c> names when <c>--report-gh</c> is passed
/// and <c>GITHUB_ACTIONS</c> is <c>true</c>. This example sets both variables itself, as a runner of GitHub
/// Actions does. The option needs <c>EnableMicrosoftTestingExtensionsGitHubActionsReport</c> in the project
/// file.
/// </summary>
[TestClass]
public sealed class VerdictRunTests(TestContext testContext)
{
    private const int ZeroTests = 8;
    private const int MinimumExpectedTestsPolicyViolation = 9;

    private static readonly string Assembly = typeof(VerdictRunTests).Assembly.GetName().Name!;

    [TestMethod]
    public async Task Passed_run_has_a_passed_heading_and_the_counts_of_the_assembly()
    {
        var (exitCode, summary) = await SummarizeAsync("--filter", $"ClassName={typeof(BasketTests).FullName}");

        Assert.AreEqual(0, exitCode);
        Assert.Contains($"## ✅ Test Run Summary — {Assembly} (net10.0)", summary);
        Assert.Contains("| Total | Passed | Failed | Skipped | Flaky | Duration |", summary);
        Assert.Contains("| 2 | 2 | 0 | 0 | 0 |", summary);
        Assert.DoesNotContain("Exit code", summary);
    }

    [TestMethod]
    public async Task Run_with_too_few_tests_has_a_failed_heading_and_a_warning_with_the_exit_code()
    {
        var (exitCode, summary) = await SummarizeAsync(
            "--filter", $"ClassName={typeof(BasketTests).FullName}",
            "--minimum-expected-tests", "3");

        Assert.AreEqual(MinimumExpectedTestsPolicyViolation, exitCode);
        Assert.Contains($"## ❌ Test Run Summary — {Assembly} (net10.0)", summary);
        Assert.Contains("| 2 | 2 | 0 | 0 | 0 |", summary);
        Assert.Contains(
            "> [!WARNING]\n> Exit code 9 — MinimumExpectedTestsPolicyViolation: Fewer tests ran than required by --minimum-expected-tests.",
            summary);
    }

    [TestMethod]
    public async Task Run_without_tests_has_a_failed_heading_and_a_warning_with_the_exit_code()
    {
        var (exitCode, summary) = await SummarizeAsync("--filter", "ClassName=No.Such.Tests");

        Assert.AreEqual(ZeroTests, exitCode);
        Assert.Contains($"## ❌ Test Run Summary — {Assembly} (net10.0)", summary);
        Assert.Contains("| 0 | 0 | 0 | 0 | 0 |", summary);
        Assert.Contains(
            "> [!WARNING]\n> Exit code 8 — ZeroTests: No tests were found to run. This often means a test filter matched nothing.",
            summary);
    }

    private async Task<(int ExitCode, string Summary)> SummarizeAsync(params string[] arguments)
    {
        var results = Directory.CreateTempSubdirectory();
        var summary = Path.Combine(results.FullName, "summary.md");
        var runner = new Dictionary<string, string> { ["GITHUB_ACTIONS"] = "true", ["GITHUB_STEP_SUMMARY"] = summary };

        using var run = await TestApplicationRun.StartAsync(results, runner, testContext.CancellationToken, [.. arguments, "--report-gh"]);

        return (run.ExitCode, await File.ReadAllTextAsync(summary, testContext.CancellationToken));
    }
}
