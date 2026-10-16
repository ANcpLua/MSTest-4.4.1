namespace MSTest.Isolation.ResourceLocks;

/// <summary>
///     Readers wait for a writer only. Mode = ResourceAccessMode.ReadWrite, which is also the mode of a lock that
///     names none, keeps every reader and every other writer of the resource out while the test runs.
/// </summary>
[TestClass]
public sealed class ReadersAndWriterTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    private static int _readers;

    private static int _writers;

    [TestMethod]
    [ResourceLock(SharedResources.Inventory, Mode = ResourceAccessMode.Read)]
    public async Task Counts_the_stock_without_meeting_the_writer()
    {
        Interlocked.Increment(ref _readers);
        try
        {
            Assert.AreEqual(0, Volatile.Read(ref _writers));

            await Task.Delay(Hold, testContext.CancellationToken);

            Assert.AreEqual(0, Volatile.Read(ref _writers));
        }
        finally
        {
            Interlocked.Decrement(ref _readers);
        }
    }

    [TestMethod]
    [ResourceLock(SharedResources.Inventory, Mode = ResourceAccessMode.Read)]
    public async Task Lists_the_stock_without_meeting_the_writer()
    {
        Interlocked.Increment(ref _readers);
        try
        {
            Assert.AreEqual(0, Volatile.Read(ref _writers));

            await Task.Delay(Hold, testContext.CancellationToken);

            Assert.AreEqual(0, Volatile.Read(ref _writers));
        }
        finally
        {
            Interlocked.Decrement(ref _readers);
        }
    }

    [TestMethod]
    [ResourceLock(SharedResources.Inventory, Mode = ResourceAccessMode.ReadWrite)]
    public async Task Restocks_without_meeting_a_reader()
    {
        var active = Interlocked.Increment(ref _writers);
        try
        {
            Assert.AreEqual(1, active);
            Assert.AreEqual(0, Volatile.Read(ref _readers));

            await Task.Delay(Hold, testContext.CancellationToken);

            Assert.AreEqual(0, Volatile.Read(ref _readers));
        }
        finally
        {
            Interlocked.Decrement(ref _writers);
        }
    }
}