using MSTest.Showcase;

namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>
/// Two tests that wait until the run is cancelled and then clean up in <c>finally</c>, one in two seconds and
/// one in two minutes. Each writes <c>waiting</c> into the directory that the variable <c>MSTEST_DEPOT</c>
/// names when it starts to wait, and <c>handed-over</c> when its cleanup is done. Expected outcome with
/// MSTEST_EXPLICIT=1, MSTEST_DEPOT set to an empty directory, a filter on Hands_over_before_stopping and
/// <c>--timeout 5s</c>: the run is cancelled after five seconds, the cleanup writes <c>handed-over</c>, the test
/// is not reported as passed or failed and the run ends with exit code 3.
/// </summary>
[TestClass]
[Explicit]
public sealed class HandoverTests(TestContext testContext)
{
    public const string Depot = "MSTEST_DEPOT";
    public const string Waiting = "waiting";
    public const string HandedOver = "handed-over";

    [TestMethod]
    public Task Hands_over_before_stopping() => WaitForTheEndOfTheShiftAsync(TimeSpan.FromSeconds(2));

    [TestMethod]
    public Task Stalls_while_stopping() => WaitForTheEndOfTheShiftAsync(TimeSpan.FromMinutes(2));

    private async Task WaitForTheEndOfTheShiftAsync(TimeSpan handover)
    {
        var depot = Environment.GetEnvironmentVariable(Depot)!;
        try
        {
            await File.WriteAllTextAsync(Path.Combine(depot, Waiting), string.Empty, CancellationToken.None);
            await Task.Delay(Timeout.InfiniteTimeSpan, testContext.CancellationToken);
        }
        finally
        {
            await Task.Delay(handover, CancellationToken.None);
            await File.WriteAllTextAsync(Path.Combine(depot, HandedOver), string.Empty, CancellationToken.None);
        }
    }
}
