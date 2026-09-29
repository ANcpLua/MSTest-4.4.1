namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// Tests about a checkout, with a fixture of the class, a fixture of each test and a test that writes to the
/// console. With the provider that <c>Program.cs</c> registers, the platform records a span and the
/// instruments for each test, and MSTest records a span for each test method and each call of a fixture.
/// </summary>
[TestClass]
public sealed class CheckoutTests
{
    private static decimal[] _prices = [];

    private decimal _total;

    [ClassInitialize]
    public static void PriceTheItems(TestContext testContext) => _prices = [19.99m, 5.01m];

    [TestInitialize]
    public void TotalTheItems() => _total = _prices.Sum();

    [TestMethod]
    public void Totals_the_items() => Assert.AreEqual(25.00m, _total);

    [TestMethod]
    public void Prints_the_receipt()
    {
        Console.WriteLine("Receipt of two items");

        Assert.HasCount(2, _prices);
    }
}
