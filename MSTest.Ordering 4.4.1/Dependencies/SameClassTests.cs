using System.Collections.Concurrent;

namespace MSTest.Ordering.Dependencies;

/// <summary>
/// [DependsOn(nameof(...))] names a test of the same class that has to finish before the annotated test
/// starts. The tests are declared in the reverse of the order they run in, and each one finds exactly the
/// tests before it finished.
/// </summary>
[TestClass]
public sealed class SameClassTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static readonly ConcurrentQueue<string> Finished = new();

    [TestMethod]
    [DependsOn(nameof(PayOrder))]
    public void ShipOrder() => Assert.AreSequenceEqual([nameof(CreateOrder), nameof(PayOrder)], Finished);

    [TestMethod]
    [DependsOn(nameof(CreateOrder))]
    public async Task PayOrder()
    {
        Assert.AreSequenceEqual([nameof(CreateOrder)], Finished);

        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(PayOrder));
    }

    [TestMethod]
    public async Task CreateOrder()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(CreateOrder));
    }
}
