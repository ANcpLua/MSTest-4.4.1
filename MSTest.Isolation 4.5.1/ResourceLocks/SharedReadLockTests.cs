namespace MSTest.Isolation.ResourceLocks;

/// <summary>
/// Mode = ResourceAccessMode.Read declares that a test only reads the resource. Tests that read the same
/// resource run together: each of the two readers waits, inside its lock, until the other one has arrived.
/// </summary>
[TestClass]
public sealed class SharedReadLockTests(TestContext testContext)
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    private static readonly SemaphoreSlim FirstIsReading = new(0);

    private static readonly SemaphoreSlim SecondIsReading = new(0);

    [TestMethod]
    [ResourceLock(SharedResources.Catalog, Mode = ResourceAccessMode.Read)]
    public async Task First_reader_meets_the_second()
    {
        FirstIsReading.Release();

        Assert.IsTrue(await SecondIsReading.WaitAsync(Patience, testContext.CancellationToken));
    }

    [TestMethod]
    [ResourceLock(SharedResources.Catalog, Mode = ResourceAccessMode.Read)]
    public async Task Second_reader_meets_the_first()
    {
        SecondIsReading.Release();

        Assert.IsTrue(await FirstIsReading.WaitAsync(Patience, testContext.CancellationToken));
    }
    [ClassCleanup]
    public static void DisposeSignals()
    {
        FirstIsReading.Dispose();
        SecondIsReading.Dispose();
    }

}
