using System.Text.Json.Nodes;
using System.Xml.Linq;
using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Retries;

/// <summary>
/// The JUnit and the CTRF report of a run that <c>--retry-failed-tests</c> repeats hold every test of the run
/// with its final outcome, although the last attempt runs only the tests that failed before. A test that passed
/// in the first attempt stays in the reports. Here <see cref="RestockTests"/> run with one retry, whose second
/// attempt runs Finds_the_item_after_the_restock alone. The options need
/// <c>EnableMicrosoftTestingExtensionsRetry</c>, <c>EnableMicrosoftTestingExtensionsJUnitReport</c> and
/// <c>EnableMicrosoftTestingExtensionsCtrfReport</c> in the project file.
/// </summary>
[TestClass]
public sealed class ConsolidatedReportRunTests
{
    private const string JUnit = "restock.xml";
    private const string Ctrf = "restock.ctrf.json";

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheRestockWithOneRetry(TestContext testContext)
    {
        var results = Directory.CreateTempSubdirectory();

        _run = await TestApplicationRun.StartAsync(
            Environment.ProcessPath!,
            results,
            new Dictionary<string, string> { [RestockTests.Shelf] = results.CreateSubdirectory("shelf").FullName },
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(RestockTests).FullName}",
            "--retry-failed-tests", "1",
            "--report-junit", "--report-junit-filename", JUnit,
            "--report-ctrf", "--report-ctrf-filename", Ctrf);
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void JUnit_report_holds_both_tests_as_passed()
    {
        var cases = XDocument.Load(_run.PathOf(JUnit)).Descendants("testcase").ToArray();

        Assert.AreSequenceEqual(
            [nameof(RestockTests.Finds_the_item_on_the_shelf), nameof(RestockTests.Finds_the_item_after_the_restock)],
            cases.Select(static testCase => testCase.Attribute("name")!.Value), SequenceOrder.InAnyOrder);
        Assert.IsEmpty(cases.Elements("failure"));
    }

    [TestMethod]
    public void CTRF_report_holds_both_tests_as_passed_and_the_retried_one_as_flaky()
    {
        var results = JsonNode.Parse(File.ReadAllText(_run.PathOf(Ctrf)))!["results"]!;
        var tests = results["tests"]!.AsArray().ToDictionary(static test => (string)test!["name"]!);

        Assert.AreEqual(2, (int)results["summary"]!["passed"]!);
        Assert.AreEqual("passed", (string?)tests[nameof(RestockTests.Finds_the_item_on_the_shelf)]!["status"]);
        Assert.AreEqual("passed", (string?)tests[nameof(RestockTests.Finds_the_item_after_the_restock)]!["status"]);
        Assert.IsTrue((bool)tests[nameof(RestockTests.Finds_the_item_after_the_restock)]!["flaky"]!);
    }

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);
}
