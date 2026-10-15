namespace MSTest.Ordering.Cycles;

/// <summary>
///     The accepted form of the dependencies that the showcases of this project close to a cycle: Unlock waits
///     for Lock, and Lock waits for no test.
/// </summary>
[TestClass]
public sealed class LockTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static volatile bool _locked;

    [TestMethod]
    [DependsOn(nameof(Lock))]
    public void Unlock()
    {
        Assert.IsTrue(_locked);
    }

    [TestMethod]
    public async Task Lock()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        _locked = true;
    }
}