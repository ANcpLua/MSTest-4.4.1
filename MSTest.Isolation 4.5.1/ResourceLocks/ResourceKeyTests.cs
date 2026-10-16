namespace MSTest.Isolation.ResourceLocks;

/// <summary>
/// The key of a lock is compared character by character, capitals included: "scanner" and "SCANNER" are two
/// resources, and the tests that hold them run together. A key of white space, or none at all, is rejected
/// by the attribute.
/// </summary>
[TestClass]
public sealed class ResourceKeyTests(TestContext testContext)
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    private static readonly SemaphoreSlim ScannerIsHeld = new(0);

    private static readonly SemaphoreSlim ScannerInCapitalsIsHeld = new(0);

    [TestMethod]
    [ResourceLock(SharedResources.Scanner)]
    public async Task Holds_the_scanner_while_the_other_key_is_held()
    {
        ScannerIsHeld.Release();

        Assert.IsTrue(await ScannerInCapitalsIsHeld.WaitAsync(Patience, testContext.CancellationToken));
    }

    [TestMethod]
    [ResourceLock(SharedResources.ScannerInCapitals)]
    public async Task Holds_the_key_in_capitals_while_the_scanner_is_held()
    {
        ScannerInCapitalsIsHeld.Release();

        Assert.IsTrue(await ScannerIsHeld.WaitAsync(Patience, testContext.CancellationToken));
    }

    [TestMethod]
    public void Rejects_a_key_of_white_space()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ResourceLockAttribute(" "));
    }

    [TestMethod]
    public void Rejects_a_missing_key()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new ResourceLockAttribute(null!));
    }
    [ClassCleanup]
    public static void DisposeSignals()
    {
        ScannerIsHeld.Dispose();
        ScannerInCapitalsIsHeld.Dispose();
    }

}
