using MSTest.Showcase;

namespace MSTest.Reporting.Assertions;

/// <summary>
///     A test whose body fails a comparison and whose cleanup fails with <c>Assert.Fail</c>. A failure without
///     values does not compete with the comparison, so the failure of the test reports the values of the comparison.
///     Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: the test fails and the run ends with
///     exit code 2.
/// </summary>
// Deliberate comparison failures exercise diagnostic and structured-value reporting; Assert.Fail cannot replace them.
#pragma warning disable MSTEST0025
[TestClass]
[Explicit]
public sealed class SingleComparisonTests
{
    private const decimal Total = 12.5m;

    [TestMethod]
    public void Fails_in_the_test_and_without_values_in_the_cleanup()
    {
        Assert.AreEqual(15m, Total);
    }

    [TestCleanup]
    public void CheckTheBasket()
    {
        Assert.Fail("The basket is not checked.");
    }
}
#pragma warning restore MSTEST0025
