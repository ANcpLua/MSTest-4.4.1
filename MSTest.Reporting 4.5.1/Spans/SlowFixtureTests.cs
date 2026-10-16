using MSTest.Showcase;

namespace MSTest.Reporting.Spans;

/// <summary>
/// A test fixture that runs into its timeout, for the span of a fixture method that times out.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Is_not_reached_after_the_timeout fails
/// with the message that WaitForTheDelivery timed out after 100ms, and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class SlowFixtureTests(TestContext testContext)
{
    [TestInitialize]
    [Timeout(100, CooperativeCancellation = true)]
    public Task WaitForTheDelivery() => Task.Delay(TimeSpan.FromSeconds(5), testContext.CancellationToken);

    [TestMethod]
    public void Is_not_reached_after_the_timeout() => Assert.Fail("The test runs although its fixture timed out.");
}
