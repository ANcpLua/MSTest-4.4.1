namespace MSTest.Isolation.ClassLevel.ResourceLocks;

internal static class PriceList
{
    public static int Editors;

    public static int Readers;
}

/// <summary>
/// With ExecutionScope.ClassLevel the locks of all tests of a class are taken for the class as a whole, each
/// in the strongest mode one of them declares. One test reads the price list and another writes it, so the
/// class holds the list exclusively during both and never meets the report.
/// </summary>
[TestClass]
public sealed class PriceListMaintenanceTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    [TestMethod]
    [ResourceLock(SharedResources.PriceList, Mode = ResourceAccessMode.Read)]
    public async Task Looks_up_a_price_without_meeting_the_report()
    {
        Interlocked.Increment(ref PriceList.Editors);
        Assert.AreEqual(0, Volatile.Read(ref PriceList.Readers));

        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(0, Volatile.Read(ref PriceList.Readers));
        Interlocked.Decrement(ref PriceList.Editors);
    }

    [TestMethod]
    [ResourceLock(SharedResources.PriceList)]
    public async Task Changes_a_price_without_meeting_the_report()
    {
        Interlocked.Increment(ref PriceList.Editors);
        Assert.AreEqual(0, Volatile.Read(ref PriceList.Readers));

        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(0, Volatile.Read(ref PriceList.Readers));
        Interlocked.Decrement(ref PriceList.Editors);
    }
}

/// <summary>
/// The report reads the price list and declares it on the class. It runs before or after the maintenance,
/// never during it.
/// </summary>
[TestClass]
[ResourceLock(SharedResources.PriceList, Mode = ResourceAccessMode.Read)]
public sealed class PriceListReportTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    [TestMethod]
    public async Task Prints_the_prices_without_meeting_the_maintenance()
    {
        Interlocked.Increment(ref PriceList.Readers);
        Assert.AreEqual(0, Volatile.Read(ref PriceList.Editors));

        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(0, Volatile.Read(ref PriceList.Editors));
        Interlocked.Decrement(ref PriceList.Readers);
    }
}
