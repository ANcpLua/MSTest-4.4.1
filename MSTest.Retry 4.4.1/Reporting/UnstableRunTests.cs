using System.Text.Json;
using System.Xml.Linq;

namespace MSTest.Retry.Reporting;

/// <summary>
/// What the test host reports about a run of <see cref="UnstableTests"/>. The terminal prints every attempt
/// with its try number, the summary counts and lists the tests that passed after a retry, and the CTRF report
/// keeps the failed attempts of each test. The exit code, the TRX report and the JUnit report follow the last
/// attempt only. <c>--output Detailed</c> adds the passed results to the terminal. The CTRF and JUnit options
/// need <c>EnableMicrosoftTestingExtensionsCtrfReport</c> and <c>EnableMicrosoftTestingExtensionsJUnitReport</c>
/// in the project file.
/// </summary>
[TestClass]
public sealed class UnstableRunTests
{
    private const string First = nameof(UnstableTests.Passes_on_the_first_attempt);
    private const string Second = nameof(UnstableTests.Passes_on_the_second_attempt);
    private const string Third = nameof(UnstableTests.Passes_on_the_third_attempt);
    private const string Ctrf = "unstable.ctrf.json";
    private const string Trx = "unstable.trx";
    private const string JUnit = "unstable.xml";

    private static DirectoryInfo _reports = null!;
    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheUnstableTests(TestContext testContext)
    {
        _reports = Directory.CreateTempSubdirectory();
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(UnstableTests).FullName}",
            "--output", "Detailed",
            "--results-directory", _reports.FullName,
            "--report-ctrf", "--report-ctrf-filename", Ctrf,
            "--report-trx", "--report-trx-filename", Trx,
            "--report-junit", "--report-junit-filename", JUnit);
    }

    [ClassCleanup]
    public static void DeleteTheReports() => _reports.Delete(recursive: true);

    [TestMethod]
    public void Terminal_prints_every_attempt_with_its_try_number()
    {
        Assert.AreSequenceEqual(["failed (try 1)", "passed (try 2)"], _run.AttemptsOf(Second));
        Assert.AreSequenceEqual(["failed (try 1)", "failed (try 2)", "passed (try 3)"], _run.AttemptsOf(Third));
    }

    [TestMethod]
    public void Terminal_prints_no_try_number_for_a_test_that_passed_at_once()
    {
        Assert.IsEmpty(_run.AttemptsOf(First));
        Assert.Contains($"passed {First}", _run.Output);
    }

    [TestMethod]
    public void Summary_counts_each_test_once_and_adds_the_flaky_and_retried_lines()
    {
        Assert.Contains("  total: 3", _run.Output);
        Assert.Contains("  failed: 0", _run.Output);
        Assert.Contains("  succeeded: 3", _run.Output);
        Assert.Contains("  flaky: 2 (passed after retry)", _run.Output);
        Assert.Contains("  retried: 2 test(s), 3 extra run(s)", _run.Output);
    }

    [TestMethod]
    public void Summary_lists_the_flaky_tests_by_name()
    {
        Assert.Contains("Flaky tests:", _run.Output);
        Assert.Contains($"  {Second} failed -> passed (2 attempts)", _run.Output);
        Assert.Contains($"  {Third} failed -> passed (3 attempts)", _run.Output);
        Assert.DoesNotContain($"  {First} failed -> passed", _run.Output);
    }

    [TestMethod]
    public void Exit_code_ignores_the_failed_attempts_of_a_test_that_passed() => Assert.AreEqual(0, _run.ExitCode);

    [TestMethod]
    public void Ctrf_report_marks_the_test_as_flaky_and_keeps_its_failed_attempts()
    {
        using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(_reports.FullName, Ctrf)));
        var results = report.RootElement.GetProperty("results");
        var tests = results.GetProperty("tests").EnumerateArray().ToDictionary(static test => test.GetProperty("name").GetString()!);

        Assert.AreEqual(2, results.GetProperty("summary").GetProperty("flaky").GetInt32());
        Assert.AreEqual("passed", tests[Third].GetProperty("status").GetString());
        Assert.IsTrue(tests[Third].GetProperty("flaky").GetBoolean());
        Assert.AreEqual(2, tests[Third].GetProperty("retries").GetInt32());
        Assert.AreSequenceEqual(
            ["1: failed", "2: failed"],
            tests[Third].GetProperty("retryAttempts").EnumerateArray().Select(static attempt => $"{attempt.GetProperty("attempt")}: {attempt.GetProperty("status")}"));
        Assert.IsFalse(tests[First].TryGetProperty("flaky", out _));
        Assert.IsFalse(tests[First].TryGetProperty("retries", out _));
        Assert.IsFalse(tests[First].TryGetProperty("retryAttempts", out _));
    }

    [TestMethod]
    public void Trx_report_has_one_row_per_test_with_the_outcome_of_its_last_attempt()
    {
        XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var rows = XDocument.Load(Path.Combine(_reports.FullName, Trx))
            .Descendants(trx + "UnitTestResult")
            .Select(static row => $"{row.Attribute("testName")!.Value}: {row.Attribute("outcome")!.Value}");

        Assert.AreSequenceEqual([$"{First}: Passed", $"{Second}: Passed", $"{Third}: Passed"], rows, SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public void JUnit_report_has_one_row_per_test_and_no_failure()
    {
        var report = XDocument.Load(Path.Combine(_reports.FullName, JUnit));
        var rows = report.Descendants("testcase").Select(static row => row.Attribute("name")!.Value);

        Assert.AreSequenceEqual([First, Second, Third], rows, SequenceOrder.InAnyOrder);
        Assert.IsEmpty(report.Descendants("failure"));
    }
}
