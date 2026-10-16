namespace MSTest.Isolation.ResourceLocks;

/// <summary>
///     [ResourceLock] on a class applies to every test of the class. A test holds its locks from TestInitialize
///     to TestCleanup, so the printer taken in TestInitialize is free again before the next test takes it.
/// </summary>
[TestClass]
[ResourceLock(SharedResources.Printer)]
public sealed class ClassLockTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    private static int _users;

    [TestInitialize]
    public void TakeThePrinter()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref _users));
    }

    [TestCleanup]
    public void LeaveThePrinter()
    {
        Assert.AreEqual(0, Interlocked.Decrement(ref _users));
    }

    [TestMethod]
    public async Task Prints_the_invoice()
    {
        Assert.AreEqual(1, Volatile.Read(ref _users));
        await Task.Delay(Hold, testContext.CancellationToken);
        Assert.AreEqual(1, Volatile.Read(ref _users));
    }

    [TestMethod]
    public async Task Prints_the_receipt()
    {
        Assert.AreEqual(1, Volatile.Read(ref _users));
        await Task.Delay(Hold, testContext.CancellationToken);
        Assert.AreEqual(1, Volatile.Read(ref _users));
    }
}