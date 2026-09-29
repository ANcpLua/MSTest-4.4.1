using System.Xml.Linq;

namespace MSTest.Platform.Changed.TrxRecovery;

/// <summary>
/// <c>--report-trx</c> alone runs the tests under a controller: the process that is started becomes the
/// controller, starts the test host as a process of its own and outlives it. The summary of the run
/// therefore lists the TRX report under the artifacts that are produced out of process, the ones that the
/// controller reports. <c>--report-trx</c> is part of the default profile of MSTest.Sdk.
/// </summary>
[TestClass]
public sealed class ControllerRunTests
{
    private const string Trx = "stocktaking.trx";

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheStocktaking(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(StocktakingTests).FullName}",
            "--report-trx", "--report-trx-filename", Trx);

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Controller_reports_the_TRX_report() =>
        Assert.Contains(
            $"  Out of process file artifacts produced:{Environment.NewLine}    - {_run.PathOf(Trx)}{Environment.NewLine}",
            _run.Output);

    [TestMethod]
    public void TRX_report_has_the_result_of_the_test()
    {
        XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var rows = XDocument.Load(_run.PathOf(Trx))
            .Descendants(trx + "UnitTestResult")
            .Select(static row => $"{row.Attribute("testName")!.Value}: {row.Attribute("outcome")!.Value}");

        Assert.AreSequenceEqual([$"{nameof(StocktakingTests.Counts_the_stock)}: Passed"], rows);
    }

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);
}
