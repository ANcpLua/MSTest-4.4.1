using MSTest.Showcase;

namespace MSTest.Platform.ServerMode.Catalogue;

/// <summary>
/// A comparison that fails, for a client that reads the compared values from the node of the test. Expected
/// outcome with MSTEST_EXPLICIT=1 and a filter on this class: the test fails, expected 119 and actual 107.00,
/// and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class GrossPriceTests
{
    private static readonly decimal Net = 100m;

    [TestMethod]
    public void Adds_the_tax_to_the_net_price() => Assert.AreEqual(119m, Net * 1.07m);
}
