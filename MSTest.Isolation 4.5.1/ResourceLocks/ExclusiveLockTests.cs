namespace MSTest.Isolation.ResourceLocks;

/// <summary>
///     [ResourceLock] names a resource that tests share. Without a mode the lock is exclusive: of the tests that
///     name the same resource, one runs at a time.
/// </summary>
[TestClass]
public sealed class ExclusiveLockTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    private static int _bookings;

    [TestMethod]
    [ResourceLock(SharedResources.Ledger)]
    public async Task Books_the_deposit_alone()
    {
        var active = Interlocked.Increment(ref _bookings);
        try
        {
            Assert.AreEqual(1, active);
            await Task.Delay(Hold, testContext.CancellationToken);
        }
        finally
        {
            Interlocked.Decrement(ref _bookings);
        }
    }

    [TestMethod]
    [ResourceLock(SharedResources.Ledger)]
    public async Task Books_the_withdrawal_alone()
    {
        var active = Interlocked.Increment(ref _bookings);
        try
        {
            Assert.AreEqual(1, active);
            await Task.Delay(Hold, testContext.CancellationToken);
        }
        finally
        {
            Interlocked.Decrement(ref _bookings);
        }
    }

    [TestMethod]
    [ResourceLock(SharedResources.Ledger)]
    public async Task Books_the_transfer_alone()
    {
        var active = Interlocked.Increment(ref _bookings);
        try
        {
            Assert.AreEqual(1, active);
            await Task.Delay(Hold, testContext.CancellationToken);
        }
        finally
        {
            Interlocked.Decrement(ref _bookings);
        }
    }
}