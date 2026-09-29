using System.Xml.Linq;

namespace MSTest.Retry.Reporting;

/// <summary>
/// What the test host reports about a run of <see cref="ExhaustedRetriesTests"/>. The test is retried but not
/// flaky, because its last attempt failed as well: the terminal prints three failed attempts, the summary has
/// a retried line and no flaky line, the run ends with exit code 2, and the TRX and JUnit reports have one
/// failed row for the three attempts.
/// </summary>
[TestClass]
public sealed class ExhaustedRetriesRunTests
{
    private const string Test = nameof(ExhaustedRetriesTests.Fails_on_every_attempt);
    private const string Trx = "exhausted.trx";
    private const string JUnit = "exhausted.xml";
    private const int Attempts = 3;
    private const int AtLeastOneTestFailed = 2;

    private static DirectoryInfo _reports = null!;
    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheFailingTest(TestContext testContext)
    {
        _reports = Directory.CreateTempSubdirectory();
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(ExhaustedRetriesTests).FullName}",
            "--results-directory", _reports.FullName,
            "--report-trx", "--report-trx-filename", Trx,
            "--report-junit", "--report-junit-filename", JUnit);
    }

    [ClassCleanup]
    public static void DeleteTheReports() => _reports.Delete(recursive: true);

    [TestMethod]
    public void Terminal_prints_the_last_attempt_with_its_try_number_as_well() =>
        Assert.AreSequenceEqual(["failed (try 1)", "failed (try 2)", "failed (try 3)"], _run.AttemptsOf(Test));

    [TestMethod]
    public void Summary_counts_the_test_as_retried_but_not_as_flaky()
    {
        Assert.Contains("  total: 1", _run.Output);
        Assert.Contains("  failed: 1", _run.Output);
        Assert.Contains("  retried: 1 test(s), 2 extra run(s)", _run.Output);
        Assert.DoesNotContain("flaky:", _run.Output);
        Assert.DoesNotContain("Flaky tests:", _run.Output);
    }

    [TestMethod]
    public void Exit_code_follows_the_last_attempt()
    {
        Assert.HasCount(Attempts, _run.AttemptsOf(Test));
        Assert.AreEqual(AtLeastOneTestFailed, _run.ExitCode);
    }

    [TestMethod]
    public void Trx_report_has_one_failed_row_for_the_three_attempts()
    {
        XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var rows = XDocument.Load(Path.Combine(_reports.FullName, Trx))
            .Descendants(trx + "UnitTestResult")
            .Select(static row => $"{row.Attribute("testName")!.Value}: {row.Attribute("outcome")!.Value}");

        Assert.HasCount(Attempts, _run.AttemptsOf(Test));
        Assert.AreSequenceEqual([$"{Test}: Failed"], rows);
    }

    [TestMethod]
    public void JUnit_report_has_one_failed_row_for_the_three_attempts()
    {
        var report = XDocument.Load(Path.Combine(_reports.FullName, JUnit));

        Assert.HasCount(Attempts, _run.AttemptsOf(Test));
        Assert.AreEqual(Test, Assert.ContainsSingle(report.Descendants("testcase")).Attribute("name")!.Value);
        Assert.ContainsSingle(report.Descendants("failure"));
    }
}
