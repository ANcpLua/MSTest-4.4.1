using System.Diagnostics;

namespace MSTest.Platform.Changed.SlowTests;

/// <summary>
/// Data-driven tests whose rows run for seconds, so that a reporter of slow tests has time to name them:
/// the rows by bike pass the threshold that <see cref="GitHubNoticeRunTests"/> sets, and the row by ship
/// passes the threshold of one minute that <see cref="AzureDevOpsLineRunTests"/> cannot change.
/// </summary>
[TestClass]
public sealed class DeliveryTests(TestContext testContext)
{
    [TestMethod]
    [DataRow(3)]
    [DataRow(4)]
    public Task Arrives_by_bike(int seconds) => ArriveAsync(seconds);

    [TestMethod]
    [DataRow(63)]
    public Task Arrives_by_ship(int seconds) => ArriveAsync(seconds);

    private async Task ArriveAsync(int seconds)
    {
        var clock = Stopwatch.StartNew();

        await Task.Delay(TimeSpan.FromSeconds(seconds), testContext.CancellationToken);

        Assert.IsGreaterThanOrEqualTo(TimeSpan.FromSeconds(seconds - 1), clock.Elapsed);
    }
}
