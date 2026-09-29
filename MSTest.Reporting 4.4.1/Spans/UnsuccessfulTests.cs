using MSTest.Showcase;

namespace MSTest.Reporting.Spans;

/// <summary>
/// A test that fails and a test that is inconclusive, for the spans of tests that do not pass.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Finds_a_missing_item fails,
/// Waits_for_a_delivery is skipped and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class UnsuccessfulTests
{
    private static readonly string[] Basket = ["apple", "pear"];

    [TestMethod]
    public void Finds_a_missing_item() => Assert.Contains("plum", Basket);

    [TestMethod]
    public void Waits_for_a_delivery() => Assert.Inconclusive("The delivery has not arrived.");
}
