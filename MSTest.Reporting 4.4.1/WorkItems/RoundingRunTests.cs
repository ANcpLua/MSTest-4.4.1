using System.Xml.Linq;

namespace MSTest.Reporting.WorkItems;

/// <summary>
/// What the TRX report of a run of <see cref="RoundingTests"/> keeps of the links. The definition of a linked
/// test has a <c>Workitems</c> element with one <c>WorkItem</c> for each attribute, in the order of the
/// attributes. Its <c>id</c> is the number of the work item, for a GitHub address the number of the issue, pull
/// request or discussion. The definition of a test without a link has no <c>Workitems</c> element.
/// </summary>
[TestClass]
public sealed class RoundingRunTests
{
    private const string Report = "rounding.trx";

    private static readonly XNamespace Trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    private static TestApplicationRun _run = null!;
    private static XDocument _report = null!;

    [ClassInitialize]
    public static async Task RunTheLinkedTests(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(RoundingTests).FullName}",
            "--report-trx", "--report-trx-filename", Report);
        _report = _run.ReadReport(Report);
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Work_item_is_written_with_its_number_and_only_for_a_linked_test()
    {
        Assert.AreSequenceEqual(["1234"], WorkItemsOf(nameof(RoundingTests.Rounds_half_a_cent_up)));
        Assert.IsEmpty(DefinitionOf(nameof(RoundingTests.Rounds_less_than_half_a_cent_down)).Elements(Trx + "Workitems"));
    }

    [TestMethod]
    public void GitHub_issue_is_written_with_the_number_of_its_address() =>
        Assert.AreSequenceEqual(["5678"], WorkItemsOf(nameof(RoundingTests.Rounds_a_refund_away_from_zero)));

    [TestMethod]
    public void Several_links_are_written_in_the_order_of_the_attributes() =>
        Assert.AreSequenceEqual(["1234", "5701", "77"], WorkItemsOf(nameof(RoundingTests.Keeps_a_rounded_price)));

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);

    private static IEnumerable<string> WorkItemsOf(string test) =>
        Assert.ContainsSingle(DefinitionOf(test).Elements(Trx + "Workitems"))
            .Elements(Trx + "WorkItem")
            .Select(static workItem => workItem.Attribute("id")!.Value);

    private static XElement DefinitionOf(string test) =>
        Assert.ContainsSingle(_report.Descendants(Trx + "UnitTest").Where(definition => definition.Attribute("name")!.Value == test));
}
