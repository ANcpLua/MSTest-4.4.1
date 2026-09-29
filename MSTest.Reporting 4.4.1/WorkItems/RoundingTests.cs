namespace MSTest.Reporting.WorkItems;

/// <summary>
/// <c>[WorkItem]</c> links a test to the number of a work item and <c>[GitHubWorkItem]</c> to the address of a
/// GitHub issue, pull request or discussion. A test takes any number of both.
/// </summary>
[TestClass]
public sealed class RoundingTests
{
    [TestMethod]
    [WorkItem(1234)]
    public void Rounds_half_a_cent_up() => Assert.AreEqual(0.13m, Round(0.125m));

    [TestMethod]
    [GitHubWorkItem("https://github.com/contoso/shop/issues/5678")]
    public void Rounds_a_refund_away_from_zero() => Assert.AreEqual(-0.13m, Round(-0.125m));

    [TestMethod]
    [WorkItem(1234)]
    [GitHubWorkItem("https://github.com/contoso/shop/pull/5701")]
    [GitHubWorkItem("https://github.com/contoso/shop/discussions/77")]
    public void Keeps_a_rounded_price() => Assert.AreEqual(0.13m, Round(0.13m));

    [TestMethod]
    public void Rounds_less_than_half_a_cent_down() => Assert.AreEqual(0.12m, Round(0.124m));

    private static decimal Round(decimal price) => Math.Round(price, 2, MidpointRounding.AwayFromZero);
}
