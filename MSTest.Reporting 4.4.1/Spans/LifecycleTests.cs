using System.Diagnostics;

namespace MSTest.Reporting.Spans;

/// <summary>
/// A test class with an assembly, a class and a test initialize and cleanup method. With the OpenTelemetry
/// provider that <c>Program.cs</c> registers, MSTest records a span for each call of these six fixture methods
/// and a span for each test it runs; an ignored test does not run and has no span. None of these spans becomes
/// <see cref="Activity.Current"/>: a test runs with the <c>TestFramework</c> span of the platform as the current
/// activity.
/// </summary>
[TestClass]
public sealed class LifecycleTests
{
    private static bool _open;
    private static string[] _shelf = [];

    private readonly List<string> _basket = [];

    [AssemblyInitialize]
    public static void OpenTheShop(TestContext testContext) => _open = true;

    [AssemblyCleanup]
    public static void CloseTheShop() => _open = false;

    [ClassInitialize]
    public static void StockTheShelf(TestContext testContext) => _shelf = ["apple", "pear"];

    [ClassCleanup]
    public static void ClearTheShelf() => _shelf = [];

    [TestInitialize]
    public void FillTheBasket() => _basket.AddRange(_shelf);

    [TestCleanup]
    public void EmptyTheBasket() => _basket.Clear();

    [TestMethod]
    public void Holds_the_stock_of_the_open_shop()
    {
        Assert.IsTrue(_open);
        Assert.AreSequenceEqual(["apple", "pear"], _basket);
    }

    [TestMethod]
    [DataRow("apple")]
    [DataRow("pear")]
    public void Holds_the_item(string item) => Assert.Contains(item, _basket);

    [TestMethod(UnfoldingStrategy = TestDataSourceUnfoldingStrategy.Fold)]
    [DataRow("apple")]
    [DataRow("pear")]
    public void Holds_the_item_as_one_test_case(string item) => Assert.Contains(item, _basket);

    [TestMethod]
    [Ignore("The shop is closed on Sundays.")]
    public void Is_ignored() => Assert.IsTrue(_open);

    [TestMethod]
    public void Runs_outside_its_own_span() => Assert.AreEqual("TestFramework", Activity.Current?.OperationName);
}
