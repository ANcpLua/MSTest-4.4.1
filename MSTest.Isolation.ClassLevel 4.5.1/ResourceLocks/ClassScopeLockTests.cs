namespace MSTest.Isolation.ClassLevel.ResourceLocks;

internal static class Warehouse
{
    public static int Crews;
}

/// <summary>
/// With ExecutionScope.ClassLevel a class holds its locks from ClassInitialize to ClassCleanup. The receiving
/// crew enters the warehouse in ClassInitialize and leaves it in ClassCleanup, and in between it is the only
/// crew inside.
/// </summary>
[TestClass]
[ResourceLock(SharedResources.Warehouse)]
public sealed class ReceivingTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(100);

    [ClassInitialize]
    public static async Task EnterTheWarehouse(TestContext context)
    {
        Assert.AreEqual(1, Interlocked.Increment(ref Warehouse.Crews));

        await Task.Delay(Hold, context.CancellationToken);
    }

    [ClassCleanup]
    public static async Task LeaveTheWarehouse(TestContext context)
    {
        try
        {
            await Task.Delay(Hold, context.CancellationToken);
        }
        finally
        {
            Assert.AreEqual(0, Interlocked.Decrement(ref Warehouse.Crews));
        }
    }

    [TestMethod]
    public async Task Unloads_the_pallets_alone()
    {
        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(1, Volatile.Read(ref Warehouse.Crews));
    }

    [TestMethod]
    public async Task Counts_the_pallets_alone()
    {
        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(1, Volatile.Read(ref Warehouse.Crews));
    }
}

/// <summary>
/// The shipping crew names the same warehouse as the receiving crew. Whichever class starts second waits with
/// its ClassInitialize until the ClassCleanup of the other has ended.
/// </summary>
[TestClass]
[ResourceLock(SharedResources.Warehouse)]
public sealed class ShippingTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(100);

    [ClassInitialize]
    public static async Task EnterTheWarehouse(TestContext context)
    {
        Assert.AreEqual(1, Interlocked.Increment(ref Warehouse.Crews));

        await Task.Delay(Hold, context.CancellationToken);
    }

    [ClassCleanup]
    public static async Task LeaveTheWarehouse(TestContext context)
    {
        try
        {
            await Task.Delay(Hold, context.CancellationToken);
        }
        finally
        {
            Assert.AreEqual(0, Interlocked.Decrement(ref Warehouse.Crews));
        }
    }

    [TestMethod]
    public async Task Loads_the_parcels_alone()
    {
        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(1, Volatile.Read(ref Warehouse.Crews));
    }

    [TestMethod]
    public async Task Seals_the_parcels_alone()
    {
        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(1, Volatile.Read(ref Warehouse.Crews));
    }
}
