using System.Xml.Linq;
using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Cancellation;

/// <summary>
/// <c>--timeout</c> cancels the run after the given time. A test that does not watch for cancellation keeps its
/// test host busy until it ends, and the host then ends with the exit code of an aborted session. The TRX report,
/// which the controller writes with <c>--report-trx</c>, still holds the results that the test host completed,
/// says that the session was aborted, and the run ends with exit code 7, the code of a test host that did not end
/// normally. The test host counts the time from the moment it is built, before it discovers the tests. Here the run
/// of <see cref="JammedScannerTests"/> gets <c>--timeout 5s</c>, time enough to complete the first test.
/// </summary>
[TestClass]
public sealed class SessionTimeoutRunTests
{
    private const string Trx = "scanner.trx";
    private const int TestHostProcessExitedNonGracefully = 7;

    private static readonly XNamespace TrxNamespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheScannerWithATimeout(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(JammedScannerTests).FullName}",
            "--timeout", "5s",
            "--report-trx", "--report-trx-filename", Trx);

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Output_reports_the_recovered_results() =>
        Assert.Contains(
            "Test session was aborted; recovered 1 test result(s) from the TRX streaming sidecar (additional results that were in flight at termination time may be missing).",
            _run.Output);

    [TestMethod]
    public void TRX_report_holds_the_completed_test_and_says_that_the_session_was_aborted()
    {
        var report = XDocument.Load(_run.PathOf(Trx));
        var result = Assert.ContainsSingle(report.Descendants(TrxNamespace + "UnitTestResult"));

        Assert.AreEqual(nameof(JammedScannerTests.Scans_the_first_pallet), result.Attribute("testName")!.Value);
        Assert.AreEqual("Passed", result.Attribute("outcome")!.Value);
        Assert.Contains("was terminated because the test session was aborted.", report.Descendants(TrxNamespace + "RunInfo").Single().Value);
    }

    [TestMethod]
    public void Run_ends_with_exit_code_7()
    {
        Assert.Contains("Test application process didn't exit gracefully, exit code is '3'", _run.Output);
        Assert.AreEqual(TestHostProcessExitedNonGracefully, _run.ExitCode);
    }
}
