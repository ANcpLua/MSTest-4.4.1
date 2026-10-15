using MSTest.Showcase;

namespace MSTest.Ordering.Configuration;

/// <summary>
///     A test that testconfig.json makes wait for a failed test is skipped, whether a chain or a node names it;
///     "proceedOnFailure": true on the node lets its test run anyway.
///     Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Deploy fails, Verify and Announce
///     are skipped, Rollback passes.
/// </summary>
[TestClass]
[Explicit]
public sealed class ConfiguredFailureShowcase(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static volatile bool _deployed;

    [TestMethod]
    public void Rollback()
    {
        Assert.IsTrue(_deployed);
    }

    [TestMethod]
    public void Announce()
    {
        Assert.Fail("The test of a node does not run after a prerequisite failed.");
    }

    [TestMethod]
    public void Verify()
    {
        Assert.Fail("The next test of a chain does not run after a test of it failed.");
    }

    [TestMethod]
    public async Task Deploy()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        _deployed = true;

        Assert.Fail("deliberate failure");
    }
}