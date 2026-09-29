using MSTest.Showcase;

namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// A test that fails and a test that is inconclusive, for the spans and instruments of tests that do not pass.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Charges_the_card fails,
/// Waits_for_the_bank is skipped and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class DeclinedCardTests
{
    private const decimal Limit = 20.00m;

    [TestMethod]
    public void Charges_the_card() => Assert.IsLessThanOrEqualTo(Limit, 25.00m, "The card is declined above its limit.");

    [TestMethod]
    public void Waits_for_the_bank() => Assert.Inconclusive("The bank has not answered.");
}
