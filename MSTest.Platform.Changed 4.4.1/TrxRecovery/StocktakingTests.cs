namespace MSTest.Platform.Changed.TrxRecovery;

/// <summary>A test that passes, for the TRX report of the run that <see cref="ControllerRunTests"/> reads.</summary>
[TestClass]
public sealed class StocktakingTests
{
    private static readonly string[] Stock = ["apple", "pear"];

    [TestMethod]
    public void Counts_the_stock() => Assert.HasCount(2, Stock);
}
