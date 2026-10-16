using MSTest.Showcase;

using System.Collections.Concurrent;

namespace MSTest.Ordering.Configuration;

/// <summary>
/// A node under mstest:execution:dependencies:nodes in testconfig.json names a test and, in dependsOn, the
/// tests it waits for. Build and Analyze name the same prerequisite and wait at a barrier for each other,
/// so both pass only when they overlap. Pack and Publish list both of them and wait for both: Build
/// finishes well after Analyze, and Pack lists it first while Publish lists it second.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class NodeTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);
    private static readonly Barrier Branches = new(participantCount: 2);
    private static readonly ConcurrentQueue<string> Finished = new();

    [ClassCleanup]
    public static void DisposeBarrier() => Branches.Dispose();

    [TestMethod]
    public void Pack() =>
        Assert.AreSequenceEqual(
            [nameof(Restore), nameof(Build), nameof(Analyze)],
            Finished,
            SequenceOrder.InAnyOrder);

    [TestMethod]
    public void Publish() =>
        Assert.AreSequenceEqual(
            [nameof(Restore), nameof(Build), nameof(Analyze)],
            Finished,
            SequenceOrder.InAnyOrder);

    [TestMethod]
    public void Analyze()
    {
        Assert.Contains(nameof(Restore), Finished);
        Assert.IsTrue(Branches.SignalAndWait(Patience, testContext.CancellationToken));

        Finished.Enqueue(nameof(Analyze));
    }

    [TestMethod]
    public async Task Build()
    {
        Assert.Contains(nameof(Restore), Finished);
        Assert.IsTrue(Branches.SignalAndWait(Patience, testContext.CancellationToken));

        await Task.Delay(10 * Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(Build));
    }

    [TestMethod]
    public async Task Restore()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(Restore));
    }
}
