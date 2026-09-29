using System.Collections.Concurrent;

namespace MSTest.Ordering.Dependencies;

/// <summary>
/// Tests that name the same prerequisite become runnable together, and the method-level parallelization of
/// the assembly runs them at the same time: each branch waits at a barrier for the other one, so both pass
/// only when they overlap. A test that carries [DependsOn] several times waits for every test it names:
/// AddItem finishes well after ApplyCoupon, and PlaceOrder names it in its first [DependsOn] while
/// PrintReceipt names it in its second.
/// </summary>
[TestClass]
public sealed class FanOutFanInTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);
    private static readonly Barrier Branches = new(participantCount: 2);
    private static readonly ConcurrentQueue<string> Finished = new();

    [TestMethod]
    [DependsOn(nameof(AddItem))]
    [DependsOn(nameof(ApplyCoupon))]
    public void PlaceOrder() =>
        Assert.AreSequenceEqual(
            [nameof(CreateCart), nameof(AddItem), nameof(ApplyCoupon)],
            Finished,
            SequenceOrder.InAnyOrder);

    [TestMethod]
    [DependsOn(nameof(ApplyCoupon))]
    [DependsOn(nameof(AddItem))]
    public void PrintReceipt() =>
        Assert.AreSequenceEqual(
            [nameof(CreateCart), nameof(AddItem), nameof(ApplyCoupon)],
            Finished,
            SequenceOrder.InAnyOrder);

    [TestMethod]
    [DependsOn(nameof(CreateCart))]
    public async Task AddItem()
    {
        Assert.Contains(nameof(CreateCart), Finished);
        Assert.IsTrue(Branches.SignalAndWait(Patience, testContext.CancellationToken));

        await Task.Delay(10 * Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(AddItem));
    }

    [TestMethod]
    [DependsOn(nameof(CreateCart))]
    public void ApplyCoupon()
    {
        Assert.Contains(nameof(CreateCart), Finished);
        Assert.IsTrue(Branches.SignalAndWait(Patience, testContext.CancellationToken));

        Finished.Enqueue(nameof(ApplyCoupon));
    }

    [TestMethod]
    public async Task CreateCart()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(CreateCart));
    }
}
