namespace MSTest.Platform.Changed.Reports;

/// <summary>
/// A report with a file name of its own replaces the file of an earlier run and says so with a warning. The
/// CTRF report follows this rule as the TRX, the HTML and the JUnit report do. Here a first run reports
/// <see cref="InvoiceTests.Adds_the_items"/> and a second run reports
/// <see cref="InvoiceTests.Counts_the_items"/> into the same files of the same results directory.
/// </summary>
[TestClass]
public sealed class ExistingFileRunTests
{
    private const string Ctrf = "invoice.ctrf.json";
    private const string Trx = "invoice.trx";
    private const string Html = "invoice.html";
    private const string JUnit = "invoice.xml";

    private static TestApplicationRun _first = null!;
    private static TestApplicationRun _second = null!;

    [ClassInitialize]
    public static async Task ReportTwiceIntoTheSameFiles(TestContext testContext)
    {
        _first = await ReportAsync(Directory.CreateTempSubdirectory(), nameof(InvoiceTests.Adds_the_items), testContext);
        _second = await ReportAsync(_first.Results, nameof(InvoiceTests.Counts_the_items), testContext);
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _second.Dispose();

    [TestMethod]
    public void First_run_writes_the_reports_without_a_warning() =>
        Assert.DoesNotContain("already exists and will be overwritten", _first.Output);

    [TestMethod]
    [DataRow(Ctrf, "Warning: CTRF report file '{0}' already exists and will be overwritten.")]
    [DataRow(Trx, "Warning: Trx file '{0}' already exists and will be overwritten.")]
    [DataRow(Html, "Warning: HTML report file '{0}' already exists and will be overwritten.")]
    [DataRow(JUnit, "The JUnit report file '{0}' already exists and will be overwritten.")]
    public void Second_run_warns_that_it_replaces_the_report(string file, string warning) =>
        Assert.Contains(string.Format(warning, _second.PathOf(file)), _second.Output);

    [TestMethod]
    [DataRow(Ctrf)]
    [DataRow(Trx)]
    [DataRow(Html)]
    [DataRow(JUnit)]
    public void Report_holds_the_second_run_only(string file)
    {
        var report = File.ReadAllText(_second.PathOf(file));

        Assert.Contains(nameof(InvoiceTests.Counts_the_items), report);
        Assert.DoesNotContain(nameof(InvoiceTests.Adds_the_items), report);
    }

    [TestMethod]
    public void Both_runs_pass()
    {
        Assert.AreEqual(0, _first.ExitCode);
        Assert.AreEqual(0, _second.ExitCode);
    }

    private static Task<TestApplicationRun> ReportAsync(DirectoryInfo results, string test, TestContext testContext) =>
        TestApplicationRun.StartAsync(
            results,
            new Dictionary<string, string>(),
            testContext.CancellationToken,
            "--filter", $"FullyQualifiedName={typeof(InvoiceTests).FullName}.{test}",
            "--report-ctrf", "--report-ctrf-filename", Ctrf,
            "--report-trx", "--report-trx-filename", Trx,
            "--report-html", "--report-html-filename", Html,
            "--report-junit", "--report-junit-filename", JUnit);
}
