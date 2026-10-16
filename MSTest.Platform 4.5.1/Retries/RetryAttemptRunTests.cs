using System.Text.Json;
using System.Xml.Linq;

namespace MSTest.Platform.Retries;

/// <summary>
/// What the test host makes of the attempts of <see cref="UnsteadyTests"/>. <see cref="AttemptReporter"/>
/// receives every attempt with its number. The terminal prints every attempt with its try number and counts
/// the test once. The exit code, the TRX report and the JUnit report take the final attempt only, and the
/// CTRF report keeps the superseded attempts of a test in <c>retryAttempts</c>, counts them in
/// <c>retries</c> and marks the test as <c>flaky</c>. The Azure DevOps report writes no
/// <c>##vso[task.logissue]</c> line for the failed attempts that a later attempt supersedes; its log group
/// shows that the report is active in the run, with <c>TF_BUILD</c> set to <c>true</c>. The JUnit, the CTRF
/// and the Azure DevOps options need <c>EnableMicrosoftTestingExtensionsJUnitReport</c>,
/// <c>EnableMicrosoftTestingExtensionsCtrfReport</c> and <c>EnableMicrosoftTestingExtensionsAzureDevOpsReport</c>
/// in the project file.
/// </summary>
[TestClass]
public sealed class RetryAttemptRunTests
{
    private const string Third = nameof(UnsteadyTests.Scans_a_label_at_the_third_attempt);
    private const string Once = nameof(UnsteadyTests.Scans_a_label_at_once);
    private const string Trx = "unsteady.trx";
    private const string JUnit = "unsteady.xml";
    private const string Ctrf = "unsteady.ctrf.json";

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheUnsteadyTests(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            new Dictionary<string, string?> { ["TF_BUILD"] = "true" },
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(UnsteadyTests).FullName}",
            "--output", "detailed",
            "--report-trx", "--report-trx-filename", Trx,
            "--report-junit", "--report-junit-filename", JUnit,
            "--report-ctrf", "--report-ctrf-filename", Ctrf,
            "--report-azdo", "--report-azdo-groups", "on");

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Extension_receives_every_attempt_with_its_number()
    {
        var attempts = _run.Output.Split('\n').Where(static line => line.StartsWith("Attempt ")).Select(static line => line.Trim());

        Assert.AreSequenceEqual(
            [$"Attempt 1 of {Third}: failed, superseded", $"Attempt 2 of {Third}: failed, superseded", $"Attempt 3 of {Third}: passed, final"],
            attempts);
    }

    [TestMethod]
    public void Terminal_prints_every_attempt_with_its_try_number()
    {
        Assert.AreSequenceEqual(
            [$"failed (try 1) {Third}", $"failed (try 2) {Third}", $"passed (try 3) {Third}"],
            _run.ResultLines.Where(static line => line.EndsWith(Third)));
        Assert.Contains($"passed {Once}", _run.ResultLines);
    }

    [TestMethod]
    public void Summary_counts_the_retried_test_once() =>
        Assert.AreSequenceEqual(
            ["total: 2", "failed: 0", "succeeded: 2", "skipped: 0", "flaky: 1 (passed after retry)", "retried: 1 test(s), 2 extra run(s)"],
            _run.LinesBelow("Test run summary:").Take(6));

    [TestMethod]
    public void Exit_code_ignores_the_superseded_attempts() => Assert.AreEqual(0, _run.ExitCode);

    [TestMethod]
    public void Trx_report_has_the_final_attempt_only()
    {
        XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var results = XDocument.Load(_run.PathOf(Trx))
            .Descendants(trx + "UnitTestResult")
            .Select(static result => $"{result.Attribute("testName")!.Value}: {result.Attribute("outcome")!.Value}");

        Assert.AreSequenceEqual([$"{Third}: Passed", $"{Once}: Passed"], results, SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public void JUnit_report_has_the_final_attempt_only()
    {
        var report = XDocument.Load(_run.PathOf(JUnit));

        Assert.AreSequenceEqual([Third, Once], report.Descendants("testcase").Select(static test => test.Attribute("name")!.Value), SequenceOrder.InAnyOrder);
        Assert.IsEmpty(report.Descendants("failure"));
    }

    [TestMethod]
    public void Azure_DevOps_report_has_no_issue_for_the_superseded_attempts()
    {
        Assert.Contains("##[group]Tests: MSTest.Platform 4.5.1 (net10.0)", _run.Output);
        Assert.DoesNotContain("##vso[task.logissue", _run.Output);
    }

    [TestMethod]
    public void Ctrf_report_keeps_the_superseded_attempts_of_the_test()
    {
        using var report = JsonDocument.Parse(File.ReadAllText(_run.PathOf(Ctrf)));
        var results = report.RootElement.GetProperty("results");
        var tests = results.GetProperty("tests").EnumerateArray().ToDictionary(static test => test.GetProperty("name").GetString()!);

        Assert.AreEqual("passed", tests[Third].GetProperty("status").GetString());
        Assert.IsTrue(tests[Third].GetProperty("flaky").GetBoolean());
        Assert.AreEqual(2, tests[Third].GetProperty("retries").GetInt32());
        Assert.AreSequenceEqual(
            ["1: failed", "2: failed"],
            tests[Third].GetProperty("retryAttempts").EnumerateArray().Select(static attempt => $"{attempt.GetProperty("attempt")}: {attempt.GetProperty("status")}"));
        Assert.AreEqual(1, results.GetProperty("summary").GetProperty("flaky").GetInt32());
    }

    [TestMethod]
    public void Ctrf_report_adds_nothing_to_a_test_that_ran_once()
    {
        using var report = JsonDocument.Parse(File.ReadAllText(_run.PathOf(Ctrf)));
        var once = report.RootElement.GetProperty("results").GetProperty("tests").EnumerateArray().Single(static test => test.GetProperty("name").GetString() == Once);

        Assert.IsFalse(once.TryGetProperty("retries", out _));
        Assert.IsFalse(once.TryGetProperty("retryAttempts", out _));
        Assert.IsFalse(once.TryGetProperty("flaky", out _));
    }
}
