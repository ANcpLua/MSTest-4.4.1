using MSTest.Showcase;

namespace MSTest.Ordering.Cycles;

/// <summary>
///     A chain in testconfig.json that leads back to its first test is a cycle as well: the run reports it and
///     fails Lock and Unlock without running them. The third test, whose node sets proceedOnFailure, runs.
///     Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Lock and Unlock fail with
///     "Dependency cycle detected between tests", the third test passes.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("DependencyCycle")]
public sealed class ConfiguredCycleShowcase
{
    private static volatile bool _ran;

    [TestMethod]
    public void Lock()
    {
        _ran = true;
    }

    [TestMethod]
    public void Unlock()
    {
        _ran = true;
    }

    [TestMethod]
    public void Neither_test_of_the_cycle_ran()
    {
        Assert.IsFalse(_ran);
    }
}