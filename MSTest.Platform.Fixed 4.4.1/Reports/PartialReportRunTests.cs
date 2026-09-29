using System.Text.Json.Nodes;
using System.Xml.Linq;
using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Reports;

/// <summary>
/// When the test host ends before the run could write its reports, the HTML, the JUnit and the CTRF report
/// are written from the results that the test host completed. Each report is marked incomplete, and the output
/// says for each report how many results it recovered. The reports are written by the controller, the process
/// that starts the test host, which the platform uses on every operating system but Android, browser, WASI, iOS
/// and tvOS. Here the test host of a run of <see cref="PowerCutTests"/> ends after the first test.
/// </summary>
[TestClass]
public sealed class PartialReportRunTests
{
    private const string Html = "stock.html";
    private const string JUnit = "stock.xml";
    private const string Ctrf = "stock.ctrf.json";
    private const string Recovered =
        "The test host terminated before report generation completed. Recovered 1 terminal test result(s); the generated report is marked incomplete.";

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task CutThePowerDuringTheStocktaking(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(PowerCutTests).FullName}",
            "--report-html", "--report-html-filename", Html,
            "--report-junit", "--report-junit-filename", JUnit,
            "--report-ctrf", "--report-ctrf-filename", Ctrf);

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Output_reports_the_recovery_of_each_report() =>
        Assert.HasCount(3, _run.Output.Split(Environment.NewLine).Where(static line => line == Recovered));

    [TestMethod]
    public void JUnit_report_holds_the_completed_test_and_is_marked_incomplete()
    {
        var report = XDocument.Load(_run.PathOf(JUnit));

        Assert.AreEqual(nameof(PowerCutTests.Counts_the_stock), Assert.ContainsSingle(report.Descendants("testcase")).Attribute("name")!.Value);
        Assert.Contains(
            "true",
            report.Descendants("property").Where(static property => property.Attribute("name")!.Value == "incomplete").Select(static property => property.Attribute("value")!.Value));
    }

    [TestMethod]
    public void CTRF_report_holds_the_completed_test_and_is_marked_incomplete()
    {
        var results = JsonNode.Parse(File.ReadAllText(_run.PathOf(Ctrf)))!["results"]!;

        Assert.AreEqual(nameof(PowerCutTests.Counts_the_stock), (string?)Assert.ContainsSingle(results["tests"]!.AsArray())!["name"]);
        Assert.IsTrue((bool)results["environment"]!["extra"]!["incomplete"]!);
    }

    [TestMethod]
    public void HTML_report_holds_the_completed_test_and_is_marked_incomplete()
    {
        var page = File.ReadAllText(_run.PathOf(Html));

        Assert.Contains(nameof(PowerCutTests.Counts_the_stock), page);
        Assert.DoesNotContain(nameof(PowerCutTests.Loses_the_power), page);
        Assert.Contains("\"incomplete\":true", page);
    }

    [TestMethod]
    public void Run_ends_with_exit_code_7() => Assert.AreEqual(7, _run.ExitCode);
}
