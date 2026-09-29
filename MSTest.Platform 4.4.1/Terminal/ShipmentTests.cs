using MSTest.Showcase;

namespace MSTest.Platform.Terminal;

/// <summary>
/// Four shipments with four outcomes: one test passes, one fails, one exceeds its timeout and one is skipped.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: one test passes, two fail, one is
/// skipped and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class ShipmentTests(TestContext testContext)
{
    private const decimal Paid = 40m;

    [TestMethod]
    public void Ships_a_paid_order() => Assert.IsGreaterThan(0m, Paid);

    [TestMethod]
    public void Ships_an_order_that_costs_more_than_was_paid() => Assert.IsGreaterThanOrEqualTo(45m, Paid);

    [TestMethod]
    [Timeout(100, CooperativeCancellation = true)]
    public async Task Waits_for_a_carrier_that_never_answers() => await Task.Delay(Timeout.Infinite, testContext.CancellationToken);

    [TestMethod]
    [Ignore("The carrier has no route abroad")]
    public void Ships_abroad() => Assert.IsGreaterThan(0m, Paid);
}
