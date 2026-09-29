using MSTest.Showcase;

namespace MSTest.Platform.Assertions;

/// <summary>
/// Four failures with different values to report: a comparison of two values, a search for one value, a
/// failure without values and an exception that is no assertion. Expected outcome with MSTEST_EXPLICIT=1 and
/// a filter on this class: all four tests fail and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class PriceTests
{
    private static readonly decimal Net = 100m;
    private static readonly string[] Currencies = ["EUR", "USD"];

    [TestMethod]
    public void Adds_the_tax_to_the_net_price() => Assert.AreEqual(119m, Net * 1.07m);

    [TestMethod]
    public void Accepts_the_currency() => Assert.Contains("CHF", Currencies);

    [TestMethod]
    public void Rounds_the_price() => Assert.Fail("The rounding is not decided.");

    [TestMethod]
    public void Converts_the_price() => throw new InvalidOperationException("The rate is missing.");
}
