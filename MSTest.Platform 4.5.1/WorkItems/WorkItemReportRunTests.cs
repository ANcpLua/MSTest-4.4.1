using System.Xml.Linq;

namespace MSTest.Platform.WorkItems;

/// <summary>
/// What the TRX report of a run of <see cref="ReturnTests"/> keeps of the links. The definition of a linked
/// test has one <c>Workitems</c> element with a <c>WorkItem</c> for each link, whose <c>id</c> is the number
/// of the work item or the number at the end of the GitHub address. A test without a link has no
/// <c>Workitems</c> element.
/// </summary>
[TestClass]
public sealed class WorkItemReportRunTests
{
    private const string Report = "returns.trx";

    private static readonly XNamespace Trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    private static TestApplicationRun _run = null!;
    private static XDocument _report = null!;

    [ClassInitialize]
    public static async Task RunTheReturnTests(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(ReturnTests).FullName}",
            "--report-trx", "--report-trx-filename", Report);
        _report = XDocument.Load(_run.PathOf(Report));
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Work_item_is_kept_with_its_number() =>
        Assert.AreSequenceEqual(["4711"], WorkItemsOf(nameof(ReturnTests.Accepts_a_return_within_thirty_days)));

    [TestMethod]
    public void GitHub_address_is_kept_with_the_number_at_its_end() =>
        Assert.AreSequenceEqual(["812"], WorkItemsOf(nameof(ReturnTests.Rejects_a_return_after_thirty_days)));

    [TestMethod]
    public void Links_of_both_kinds_are_kept_in_one_list() =>
        Assert.AreSequenceEqual(["4711", "4712", "820"], WorkItemsOf(nameof(ReturnTests.Accepts_a_return_on_the_day_of_the_purchase)), SequenceOrder.InAnyOrder);

    [TestMethod]
    public void Test_without_a_link_has_no_work_items() =>
        Assert.IsEmpty(DefinitionOf(nameof(ReturnTests.Rejects_a_return_before_the_purchase)).Elements(Trx + "Workitems"));

    private static IEnumerable<string> WorkItemsOf(string test) =>
        Assert.ContainsSingle(DefinitionOf(test).Elements(Trx + "Workitems"))
            .Elements(Trx + "WorkItem")
            .Select(static workItem => workItem.Attribute("id")!.Value);

    private static XElement DefinitionOf(string test) =>
        Assert.ContainsSingle(_report.Descendants(Trx + "UnitTest").Where(definition => definition.Attribute("name")!.Value == test));
}
