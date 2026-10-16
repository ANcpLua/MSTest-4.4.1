namespace MSTest.Platform.Changed.GitHubSummary;

/// <summary>Two tests that pass, for the job summaries that <see cref="VerdictRunTests"/> reads.</summary>
[TestClass]
public sealed class BasketTests
{
    private static readonly string[] Basket = ["apple", "pear"];

    [TestMethod]
    public void Holds_the_apple() => Assert.Contains("apple", Basket);

    [TestMethod]
    public void Holds_the_pear() => Assert.Contains("pear", Basket);
}
