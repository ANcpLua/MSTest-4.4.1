namespace MSTest.Isolation.ResourceLocks;

/// <summary>
/// [ResourceLock] names a resource that tests share. Without a mode the lock is exclusive: of the tests that
/// name the same resource, one runs at a time.
/// </summary>
[TestClass]
public sealed class ExclusiveLockTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    private static int bookings;

    [TestMethod]
    [ResourceLock(SharedResources.Ledger)]
    public async Task Books_the_deposit_alone()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref bookings));

        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(0, Interlocked.Decrement(ref bookings));
    }

    [TestMethod]
    [ResourceLock(SharedResources.Ledger)]
    public async Task Books_the_withdrawal_alone()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref bookings));

        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(0, Interlocked.Decrement(ref bookings));
    }

    [TestMethod]
    [ResourceLock(SharedResources.Ledger)]
    public async Task Books_the_transfer_alone()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref bookings));

        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(0, Interlocked.Decrement(ref bookings));
    }
}
