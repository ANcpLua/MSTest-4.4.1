using System.Text.Json;
using System.Xml.Linq;

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
    private const string Trx = "recount.trx";
    private const string Ctrf = "recount.ctrf.json";

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
            "--report-gh",
            "--report-trx", "--report-trx-filename", Trx,
            "--report-ctrf", "--report-ctrf-filename", Ctrf);
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
    public void Trx_has_one_passed_result_for_the_final_attempt()
    {
        XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var report = XDocument.Load(_run.PathOf(Trx));
        var result = Assert.ContainsSingle(report.Descendants(trx + "UnitTestResult"));

        Assert.AreEqual(nameof(RecountTests.Agrees_on_the_second_count), result.Attribute("testName")!.Value);
        Assert.AreEqual("Passed", result.Attribute("outcome")!.Value);
        var counters = Assert.ContainsSingle(report.Descendants(trx + "Counters"));
        Assert.AreEqual("1", counters.Attribute("total")!.Value);
        Assert.AreEqual("1", counters.Attribute("passed")!.Value);
        Assert.AreEqual("0", counters.Attribute("failed")!.Value);
    }

    [TestMethod]
    public void Ctrf_keeps_the_failed_first_attempt_and_the_successful_retry()
    {
        using var report = JsonDocument.Parse(File.ReadAllText(_run.PathOf(Ctrf)));
        var results = report.RootElement.GetProperty("results");
        var test = Assert.ContainsSingle(results.GetProperty("tests").EnumerateArray());
        var attempt = Assert.ContainsSingle(test.GetProperty("retryAttempts").EnumerateArray());

        Assert.AreEqual(nameof(RecountTests.Agrees_on_the_second_count), test.GetProperty("name").GetString());
        Assert.AreEqual("passed", test.GetProperty("status").GetString());
        Assert.IsTrue(test.GetProperty("flaky").GetBoolean());
        Assert.AreEqual(1, test.GetProperty("retries").GetInt32());
        Assert.AreEqual(1, attempt.GetProperty("attempt").GetInt32());
        Assert.AreEqual("failed", attempt.GetProperty("status").GetString());
        Assert.AreEqual(1, results.GetProperty("summary").GetProperty("flaky").GetInt32());
    }

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);
}
