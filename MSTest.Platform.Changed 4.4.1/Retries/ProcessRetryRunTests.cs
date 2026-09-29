using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MSTest.Platform.Changed.Retries;

/// <summary>
/// What the reports keep of a run of <see cref="RestockTests"/> that <c>--retry-failed-tests</c> repeats in
/// a second test host. The HTML report of the run joins the attempts: the test that passed after a retry has
/// the outcome of its last attempt, is marked as flaky and keeps its failed attempt, and the page shows the
/// retry history. The job summary of the GitHub Actions report counts the test as flaky and lists it under
/// the flaky tests. The run keeps the summary as a fragment in its results directory as well, the artifact
/// that the post-processing of several attempts or test assemblies reads. The TRX report of the run is the
/// report of the last attempt and holds the repeated test only. The options need
/// <c>EnableMicrosoftTestingExtensionsRetry</c>,
/// <c>EnableMicrosoftTestingExtensionsHtmlReport</c> and
/// <c>EnableMicrosoftTestingExtensionsGitHubActionsReport</c> in the project file. This example sets the
/// variables <c>GITHUB_ACTIONS</c> and <c>GITHUB_STEP_SUMMARY</c> itself, as a runner of GitHub Actions does.
/// </summary>
[TestClass]
public sealed partial class ProcessRetryRunTests
{
    private const string Flaky = nameof(RestockTests.Finds_the_item_after_the_restock);
    private const string Stable = nameof(RestockTests.Finds_the_item_on_the_shelf);
    private const string Html = "restock.html";
    private const string Trx = "restock.trx";

    private static TestApplicationRun _run = null!;
    private static string _page = null!;
    private static string _summary = null!;

    [ClassInitialize]
    public static async Task RunTheRestockWithOneRetry(TestContext testContext)
    {
        var results = Directory.CreateTempSubdirectory();
        var shelf = results.CreateSubdirectory("shelf");
        var summary = Path.Combine(results.FullName, "summary.md");

        _run = await TestApplicationRun.StartAsync(
            results,
            new Dictionary<string, string>
            {
                [RestockTests.Shelf] = shelf.FullName,
                ["GITHUB_ACTIONS"] = "true",
                ["GITHUB_STEP_SUMMARY"] = summary,
            },
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(RestockTests).FullName}",
            "--retry-failed-tests", "1",
            "--report-html", "--report-html-filename", Html,
            "--report-trx", "--report-trx-filename", Trx,
            "--report-gh");
        _page = await File.ReadAllTextAsync(_run.PathOf(Html), testContext.CancellationToken);
        _summary = await File.ReadAllTextAsync(summary, testContext.CancellationToken);
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Run_passes_after_the_second_attempt()
    {
        Assert.AreEqual(0, _run.ExitCode);
        Assert.Contains("Retry summary: Passed! after 2/2 attempts", _run.Output);
    }

    [TestMethod]
    public void Html_report_counts_each_test_once_and_the_flaky_test()
    {
        var summary = DataOfThePage()["summary"]!;

        Assert.AreEqual(2, (int)summary["total"]!);
        Assert.AreEqual(2, (int)summary["passed"]!);
        Assert.AreEqual(0, (int)summary["failed"]!);
        Assert.AreEqual(1, (int)summary["flaky"]!);
    }

    [TestMethod]
    public void Html_report_keeps_the_failed_attempt_of_the_flaky_test()
    {
        var test = TestOfThePage(Flaky);
        var attempt = Assert.ContainsSingle(test["retryAttempts"]!.AsArray())!;

        Assert.AreEqual("passed", (string?)test["outcome"]);
        Assert.IsTrue((bool)test["flaky"]!);
        Assert.AreEqual(1, (int)test["retries"]!);
        Assert.AreEqual(1, (int)attempt["attempt"]!);
        Assert.AreEqual("failed", (string?)attempt["outcome"]);
        Assert.Contains("The shelf is empty until the restock.", (string)attempt["errorMessage"]!);
    }

    [TestMethod]
    public void Html_report_has_no_history_for_the_test_that_passed_at_once()
    {
        var test = TestOfThePage(Stable).AsObject();

        Assert.AreEqual("passed", (string?)test["outcome"]);
        Assert.DoesNotContain("retryAttempts", test.Select(static property => property.Key));
        Assert.DoesNotContain("flaky", test.Select(static property => property.Key));
    }

    [TestMethod]
    public void Html_page_shows_the_retry_history_and_the_flaky_badge()
    {
        Assert.Contains("Retry history", _page);
        Assert.Contains("badge flaky", _page);
    }

    [TestMethod]
    public void Job_summary_counts_and_lists_the_flaky_test()
    {
        Assert.Contains("## ✅ Overall Test Run Summary", _summary);
        Assert.Contains("| Total | Passed | Failed | Skipped | Flaky | Duration |", _summary);
        Assert.Contains("| 2 | 2 | 0 | 0 | 1 |", _summary);
        Assert.Contains($"### ⚠️ Flaky tests (1)\n\n- `{typeof(RestockTests).FullName}.{Flaky}`", _summary);
    }

    [TestMethod]
    public void Job_summary_is_kept_as_a_fragment_in_the_results_directory()
    {
        var fragment = Assert.ContainsSingle(Directory.GetFiles(_run.PathOf(".ci-summary-fragments"), "github-actions-*.json"));

        Assert.Contains(Flaky, File.ReadAllText(fragment));
    }

    [TestMethod]
    public void Trx_report_holds_the_last_attempt_only()
    {
        XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var rows = XDocument.Load(_run.PathOf(Trx))
            .Descendants(trx + "UnitTestResult")
            .Select(static row => $"{row.Attribute("testName")!.Value}: {row.Attribute("outcome")!.Value}");

        Assert.AreSequenceEqual([$"{Flaky}: Passed"], rows);
    }

    private static JsonNode TestOfThePage(string test) =>
        Assert.ContainsSingle(DataOfThePage()["tests"]!.AsArray().Where(row => (string?)row!["displayName"] == test))!;

    private static JsonNode DataOfThePage() => JsonNode.Parse(DataElement().Match(_page).Groups["data"].Value)!;

    [GeneratedRegex("""<script id="mtp-data" type="application/json">(?<data>.*?)</script>""", RegexOptions.Singleline)]
    private static partial Regex DataElement();
}
