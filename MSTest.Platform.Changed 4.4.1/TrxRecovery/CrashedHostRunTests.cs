using System.Xml.Linq;

namespace MSTest.Platform.Changed.TrxRecovery;

/// <summary>
/// The controller of a run with <c>--report-trx</c> writes the TRX report when the test host of
/// <see cref="PowerCutTests"/> ends without one. The report has the results that the test host had finished,
/// the outcome <c>Failed</c> and the note that the test host crashed, and the run ends with exit code 7.
/// A result that the test host finishes just before it ends can be missing from the report.
/// </summary>
[TestClass]
public sealed class CrashedHostRunTests
{
    private const string Trx = "power-cut.trx";
    private const int TestHostProcessExitedNonGracefully = 7;

    private static readonly XNamespace Schema = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    private static TestApplicationRun _run = null!;
    private static XDocument _report = null!;

    [ClassInitialize]
    public static async Task RunIntoThePowerCut(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(PowerCutTests).FullName}",
            "--report-trx", "--report-trx-filename", Trx);
        _report = XDocument.Load(_run.PathOf(Trx));
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Run_ends_with_the_exit_code_of_a_test_host_that_did_not_end_gracefully() =>
        Assert.AreEqual(TestHostProcessExitedNonGracefully, _run.ExitCode);

    [TestMethod]
    public void Controller_says_how_many_results_it_recovered() =>
        Assert.Contains(
            "Test host crashed; recovered 1 test result(s) from the TRX streaming sidecar "
            + "(additional results that were in flight at crash time may be missing).",
            _run.Output);

    [TestMethod]
    public void Report_has_the_result_that_was_finished_before_the_crash()
    {
        var rows = _report
            .Descendants(Schema + "UnitTestResult")
            .Select(static row => $"{row.Attribute("testName")!.Value}: {row.Attribute("outcome")!.Value}");

        Assert.AreSequenceEqual([$"{nameof(PowerCutTests.Counts_the_stock)}: Passed"], rows);
    }

    [TestMethod]
    public void Report_has_the_outcome_failed_and_the_note_of_the_crash()
    {
        var summary = Assert.ContainsSingle(_report.Descendants(Schema + "ResultSummary"));
        var note = Assert.ContainsSingle(summary.Descendants(Schema + "Text"));

        Assert.AreEqual("Failed", summary.Attribute("outcome")!.Value);
        Assert.MatchesRegex(@"^Test host process pid: \d+ crashed\.$", note.Value);
    }
}
