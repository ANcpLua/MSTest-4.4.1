namespace MSTest.Platform.ServerMode.Catalogue;

/// <summary>
/// A test that a client of the server mode discovers and runs in the examples of this project.
/// </summary>
[TestClass]
public sealed class PricingTests
{
    [TestMethod]
    public void Prices_a_book_with_its_discount() => Assert.AreEqual(18m, Price(list: 20m, discount: 0.1m));

    private static decimal Price(decimal list, decimal discount) => list * (1 - discount);
}
