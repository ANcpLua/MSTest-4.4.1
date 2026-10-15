using MSTest.Showcase;

namespace MSTest.Reporting.Assertions;

/// <summary>
///     A test whose body and whose cleanup each fail a comparison. Two pairs of values compete for the one failure
///     of the test, so the failure reports no values; its message contains both comparisons.
///     Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: the test fails and the run ends with
///     exit code 2.
/// </summary>
// Deliberate comparison failures exercise diagnostic and structured-value reporting; Assert.Fail cannot replace them.
#pragma warning disable MSTEST0025
[TestClass]
[Explicit]
public sealed class CompetingComparisonTests
{
    private const decimal Total = 12.5m;

    [TestMethod]
    public void Fails_in_the_test_and_in_the_cleanup()
    {
        Assert.AreEqual(15m, Total);
    }

    [TestCleanup]
    public void ExpectAnEmptyBasket()
    {
        Assert.AreEqual(0m, Total);
    }
}
#pragma warning restore MSTEST0025
