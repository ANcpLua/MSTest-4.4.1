using MSTest.Showcase;

namespace MSTest.Ordering.Cycles;

/// <summary>
/// MSTEST0078 reports dependencies that lead back to the test they start from. The run reports the cycle
/// as well and fails its tests without running them, whatever condition they carry, so testconfig.json
/// keeps the tests of the category DependencyCycle out of a run that names no filter.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Lock and Unlock fail with
/// "Dependency cycle detected between tests", the third test passes.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("DependencyCycle")]
public sealed class CycleShowcase
{
    private static volatile bool ran;

    [TestMethod]
#pragma warning disable MSTEST0078
    [DependsOn(nameof(Unlock))]
#pragma warning restore MSTEST0078
    public void Lock() => ran = true;

    [TestMethod]
#pragma warning disable MSTEST0078
    [DependsOn(nameof(Lock))]
#pragma warning restore MSTEST0078
    public void Unlock() => ran = true;

    [TestMethod]
    [DependsOn(nameof(Lock), ProceedOnFailure = true)]
    [DependsOn(nameof(Unlock), ProceedOnFailure = true)]
    public void Neither_test_of_the_cycle_ran() => Assert.IsFalse(ran);
}
