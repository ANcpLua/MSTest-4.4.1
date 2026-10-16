namespace MSTest.Isolation.ResourceLocks;

/// <summary>
///     MSTEST0073 reports a resource key that is written as a string literal and accepts a constant. The literal
///     below names the same resource as the constant only because it is spelled the same; a typing error in it
///     names another resource, and the two tests run together.
/// </summary>
[TestClass]
public sealed class ConstantKeyTests(TestContext testContext)
{
    private const string Archive = "archive";

    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    private static int _users;

#pragma warning disable MSTEST0073
    [TestMethod]
    [ResourceLock("archive")]
    public async Task Literal_key_shares_the_lock_with_the_named_constant()
    {
        var active = Interlocked.Increment(ref _users);
        try
        {
            Assert.AreEqual(1, active);
            await Task.Delay(Hold, testContext.CancellationToken);
        }
        finally
        {
            Interlocked.Decrement(ref _users);
        }
    }
#pragma warning restore MSTEST0073

    [TestMethod]
    [ResourceLock(Archive)]
    public async Task Named_constant_shares_the_lock_with_the_literal()
    {
        var active = Interlocked.Increment(ref _users);
        try
        {
            Assert.AreEqual(1, active);
            await Task.Delay(Hold, testContext.CancellationToken);
        }
        finally
        {
            Interlocked.Decrement(ref _users);
        }
    }
}