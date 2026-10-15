namespace MSTest.Isolation.ResourceLocks;

/// <summary>
///     A lock declared on a base class applies to the classes that derive from it. Both derived classes run the
///     inherited test, and the two runs never use the database together.
/// </summary>
[ResourceLock(SharedResources.Database)]
public abstract class DatabaseTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    private static int _connections;

    [TestMethod]
    public async Task Uses_the_database_alone()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref _connections));

        await Task.Delay(Hold, testContext.CancellationToken);

        Assert.AreEqual(0, Interlocked.Decrement(ref _connections));
    }
}

/// <summary>Runs the test of <see cref="DatabaseTests" /> under the lock the base class declares.</summary>
[TestClass]
public sealed class CustomerTableTests(TestContext testContext) : DatabaseTests(testContext);

/// <summary>Runs the test of <see cref="DatabaseTests" /> under the lock the base class declares.</summary>
[TestClass]
public sealed class OrderTableTests(TestContext testContext) : DatabaseTests(testContext);