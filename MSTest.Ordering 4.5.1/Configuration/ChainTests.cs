using MSTest.Showcase;

using System.Collections.Concurrent;

namespace MSTest.Ordering.Configuration;

/// <summary>
/// A chain under mstest:execution:dependencies:chains in testconfig.json makes each test wait for the one
/// named before it. The tests carry no attribute and are declared in the reverse of the chain.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class ChainTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static readonly ConcurrentQueue<string> Finished = new();

    [TestMethod]
    public void Load() => Assert.AreSequenceEqual([nameof(Extract), nameof(Transform)], Finished);

    [TestMethod]
    public async Task Transform()
    {
        Assert.AreSequenceEqual([nameof(Extract)], Finished);

        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(Transform));
    }

    [TestMethod]
    public async Task Extract()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(Extract));
    }
}
