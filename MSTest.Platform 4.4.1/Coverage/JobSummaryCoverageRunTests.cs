using System.Text.RegularExpressions;
using MSTest.Platform.Terminal;

namespace MSTest.Platform.Coverage;

/// <summary>
/// The job summaries of the GitHub Actions and the Azure DevOps report take the coverage of the run from the
/// platform, whichever collector reports it, here <see cref="FixedCoverageCollector"/>. Each has a section
/// <c>Code coverage</c> with the covered and the total count and the percentage of each metric of the whole
/// run, and a section <c>Coverage thresholds</c> with the actual and the required percentage and the result of
/// each check. The GitHub Actions summary goes to the file that <c>GITHUB_STEP_SUMMARY</c> names, with
/// <c>--report-gh</c> and <c>GITHUB_ACTIONS</c> set to <c>true</c>. The Azure DevOps summary goes to the
/// results directory with <c>--report-azdo --report-azdo-summary</c>, and the report prints
/// <c>##vso[task.addattachment type=Distributedtask.Core.Summary;name=…;]</c> with its path.
/// </summary>
[TestClass]
public sealed class JobSummaryCoverageRunTests(TestContext testContext)
{
    private const string Metrics = """
        | Scope | Metric | Covered | Total | Coverage |
        | --- | --- | ---: | ---: | ---: |
        | Overall | Line | 140 | 200 | 70.0% |
        | Overall | Branch | 30 | 40 | 75.0% |
        """;

    private const string Thresholds = """
        | Scope | Metric | Actual | Required | Result |
        | --- | --- | ---: | ---: | --- |
        | Overall | Line | 70.0% | 80.0% | ❌ Failed |
        """;

    private static readonly string StockCounts = $"ClassName={typeof(StockCountTests).FullName}";

    [TestMethod]
    public async Task GitHub_Actions_summary_has_the_counts_the_percentages_and_the_checks()
    {
        var results = Directory.CreateTempSubdirectory();
        var summary = Path.Combine(results.FullName, "summary.md");

        using var run = await TestApplicationRun.StartAsync(
            results,
            new Dictionary<string, string>
            {
                [FixedCoverageCollector.Variable] = "140",
                ["GITHUB_ACTIONS"] = "true",
                ["GITHUB_STEP_SUMMARY"] = summary,
            },
            testContext.CancellationToken,
            "--filter", StockCounts, "--report-gh");

        var markdown = File.ReadAllText(summary);
        Assert.Contains($"### Code coverage\n\n{Metrics.ReplaceLineEndings("\n")}", markdown);
        Assert.Contains($"#### Coverage thresholds\n\n{Thresholds.ReplaceLineEndings("\n")}", markdown);
    }

    [TestMethod]
    public async Task Azure_DevOps_summary_has_the_counts_the_percentages_and_the_checks()
    {
        using var run = await TestApplicationRun.StartAsync(
            new Dictionary<string, string> { [FixedCoverageCollector.Variable] = "140", ["TF_BUILD"] = "true" },
            testContext.CancellationToken,
            "--filter", StockCounts, "--report-azdo", "--report-azdo-summary");

        var markdown = File.ReadAllText(run.PathOf("azdo-summary-MSTest.Platform_4.4.1-net10.0-arm64.md"));
        Assert.MatchesRegex(
            @"##vso\[task\.addattachment type=Distributedtask\.Core\.Summary;name=Test results - MSTest\.Platform 4\.4\.1 \(net10\.0, arm64, attempt 1, session [0-9a-f-]{36}\);\]"
            + Regex.Escape(run.PathOf("azdo-summary-MSTest.Platform_4.4.1-net10.0-arm64.md")),
            run.Output);
        Assert.Contains($"## Code coverage\n\n{Metrics.ReplaceLineEndings("\n")}", markdown);
        Assert.Contains($"### Coverage thresholds\n\n{Thresholds.ReplaceLineEndings("\n")}", markdown);
    }
}
