namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>
/// Four tests that each take a second, for the run of <see cref="Reports.HtmlDurationRunTests"/>. The assembly
/// runs its tests in parallel at method level, so the four seconds of the tests pass in about one second.
/// </summary>
[TestClass]
public sealed class OvenTests(TestContext testContext)
{
    [TestMethod]
    public Task Bakes_the_bread() => Task.Delay(TimeSpan.FromSeconds(1), testContext.CancellationToken);

    [TestMethod]
    public Task Bakes_the_rolls() => Task.Delay(TimeSpan.FromSeconds(1), testContext.CancellationToken);

    [TestMethod]
    public Task Bakes_the_cake() => Task.Delay(TimeSpan.FromSeconds(1), testContext.CancellationToken);

    [TestMethod]
    public Task Bakes_the_pie() => Task.Delay(TimeSpan.FromSeconds(1), testContext.CancellationToken);
}
