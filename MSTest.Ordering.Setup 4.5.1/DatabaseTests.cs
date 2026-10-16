namespace MSTest.Ordering.Setup;

/// <summary>
///     A test class of a second test assembly. A dependency reaches it from a test of its own assembly;
///     MSTest.Ordering names it from another assembly to show where dependencies end.
/// </summary>
[TestClass]
public sealed class DatabaseTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static volatile bool _created;

    [TestMethod]
    [DependsOn(nameof(CreateDatabase))]
    public void SeedDatabase()
    {
        Assert.IsTrue(_created);
    }

    [TestMethod]
    public async Task CreateDatabase()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        _created = true;
    }
}