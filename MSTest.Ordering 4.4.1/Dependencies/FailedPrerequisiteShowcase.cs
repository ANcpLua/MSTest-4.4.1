using MSTest.Showcase;

namespace MSTest.Ordering.Dependencies;

/// <summary>
///     A test whose prerequisite does not pass is skipped, and so is every test that depends on it in turn.
///     ProceedOnFailure = true lets a dependent run anyway, as long as every [DependsOn] it carries sets it:
///     Refund also depends on Reserve, which passes, without ProceedOnFailure, and is skipped because of Charge.
///     Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Charge fails, Ship, Invoice and
///     Refund are skipped, Reserve and Audit pass.
/// </summary>
[TestClass]
[Explicit]
public sealed class FailedPrerequisiteShowcase(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static volatile bool _charged;

    [TestMethod]
    [DependsOn(nameof(Charge), ProceedOnFailure = true)]
    public void Audit()
    {
        Assert.IsTrue(_charged);
    }

    [TestMethod]
    [DependsOn(nameof(Charge), ProceedOnFailure = true)]
    [DependsOn(nameof(Reserve))]
    public void Refund()
    {
        Assert.Fail("One [DependsOn] without ProceedOnFailure is enough to skip the test.");
    }

    [TestMethod]
    [DependsOn(nameof(Ship))]
    public void Invoice()
    {
        Assert.Fail("A test whose prerequisite was skipped does not run.");
    }

    [TestMethod]
    [DependsOn(nameof(Charge))]
    public void Ship()
    {
        Assert.Fail("A test whose prerequisite failed does not run.");
    }

    [TestMethod]
    public async Task Charge()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        _charged = true;

        Assert.Fail("deliberate failure");
    }

    [TestMethod]
    public void Reserve()
    {
    }
}