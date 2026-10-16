using MSTest.Showcase;

namespace MSTest.Ordering.Cycles;

/// <summary>
///     MSTEST0078 reports a test that names itself in [DependsOn]. For the run it is a cycle of one test: the
///     test fails without being run.
///     Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Lock fails with "Dependency cycle
///     detected between tests", the second test passes.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("DependencyCycle")]
public sealed class SelfReferenceShowcase
{
    private static volatile bool _ran;

    [TestMethod]
#pragma warning disable MSTEST0078
    [DependsOn(nameof(Lock))]
#pragma warning restore MSTEST0078
    public void Lock() => _ran = true;

    [TestMethod]
    [DependsOn(nameof(Lock), ProceedOnFailure = true)]
    public void The_test_did_not_run()
    {
        Assert.IsFalse(_ran);
    }
}