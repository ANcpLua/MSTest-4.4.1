namespace MSTest.Isolation.ResourceLocks;

/// <summary>
/// MSTEST0073 reports a resource key that is written as a string literal and accepts a constant. The literal
/// below names the same resource as the constant only because it is spelled the same; a typing error in it
/// names another resource, and the two tests run together.
/// </summary>
[TestClass]
public sealed class ConstantKeyTests(TestContext testContext)
{
    private const string Archive = "archive";

    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    private static int users;

#pragma warning disable MSTEST0073
    [TestMethod]
    [ResourceLock("archive")]
    public async Task Literal_key_is_reported()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref users));

        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(0, Interlocked.Decrement(ref users));
    }
#pragma warning restore MSTEST0073

    [TestMethod]
    [ResourceLock(Archive)]
    public async Task Constant_key_is_accepted()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref users));

        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(0, Interlocked.Decrement(ref users));
    }
}
