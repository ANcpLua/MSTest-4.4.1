namespace MSTest.Platform.Changed.Retries;

/// <summary>
/// The job summary of the GitHub Actions report for a run of <see cref="RecountTests"/>, whose test passes on
/// the second attempt of <c>[Retry]</c>. The summary counts the test once, as passed and as flaky, and lists
/// it under the flaky tests. This example sets the variables <c>GITHUB_ACTIONS</c> and
/// <c>GITHUB_STEP_SUMMARY</c> itself, as a runner of GitHub Actions does.
/// </summary>
[TestClass]
public sealed class AttemptRetryRunTests
{
    private static TestApplicationRun _run = null!;
    private static string _summary = null!;

    [ClassInitialize]
    public static async Task RunTheRecount(TestContext testContext)
    {
        var results = Directory.CreateTempSubdirectory();
        var summary = Path.Combine(results.FullName, "summary.md");

        _run = await TestApplicationRun.StartAsync(
            results,
            new Dictionary<string, string> { ["GITHUB_ACTIONS"] = "true", ["GITHUB_STEP_SUMMARY"] = summary },
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(RecountTests).FullName}",
            "--report-gh");
        _summary = await File.ReadAllTextAsync(summary, testContext.CancellationToken);
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Job_summary_counts_the_test_once_and_as_flaky()
    {
        Assert.Contains($"## ✅ Test Run Summary — {typeof(RecountTests).Assembly.GetName().Name} (net10.0)", _summary);
        Assert.Contains("| 1 | 1 | 0 | 0 | 1 |", _summary);
    }

    [TestMethod]
    public void Job_summary_lists_the_flaky_test() =>
        Assert.Contains(
            $"### ⚠️ Flaky tests (1)\n\n- `{typeof(RecountTests).FullName}.{nameof(RecountTests.Agrees_on_the_second_count)}`",
            _summary);

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);
}
