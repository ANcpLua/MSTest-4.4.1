namespace MSTest.Platform.WorkItems;

/// <summary>
/// Tests that name the work they belong to: <c>[WorkItem]</c> takes the number of a work item and
/// <c>[GitHubWorkItem]</c> the address of an issue, a pull request or a discussion on GitHub.
/// <see cref="WorkItemReportRunTests"/> reads the links from the TRX report of a run.
/// </summary>
[TestClass]
public sealed class ReturnTests
{
    private static readonly DateOnly Bought = new(2026, 3, 2);

    [TestMethod]
    [WorkItem(4711)]
    public void Accepts_a_return_within_thirty_days() => Assert.IsTrue(IsAccepted(Bought.AddDays(30)));

    [TestMethod]
    [GitHubWorkItem("https://github.com/contoso/shop/issues/812")]
    public void Rejects_a_return_after_thirty_days() => Assert.IsFalse(IsAccepted(Bought.AddDays(31)));

    [TestMethod]
    [WorkItem(4711)]
    [WorkItem(4712)]
    [GitHubWorkItem("https://github.com/contoso/shop/pull/820")]
    public void Accepts_a_return_on_the_day_of_the_purchase() => Assert.IsTrue(IsAccepted(Bought));

    [TestMethod]
    public void Rejects_a_return_before_the_purchase() => Assert.IsFalse(IsAccepted(Bought.AddDays(-1)));

    private static bool IsAccepted(DateOnly returned) => returned >= Bought && returned <= Bought.AddDays(30);
}
