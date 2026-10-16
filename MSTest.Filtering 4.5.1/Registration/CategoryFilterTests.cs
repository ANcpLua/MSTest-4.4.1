using MSTest.Showcase;

namespace MSTest.Filtering.Registration;

/// <summary>
///     The three decisions of the registered <see cref="CategoryFilter" />. The first test runs and finds the decision
///     the filter took for it. The nightly and the retired test have a failing body, so the run passes only when the
///     filter keeps both from running: the nightly test is counted as skipped, the retired test is not counted.
/// </summary>
[TestClass]
[Explicit]
public sealed class CategoryFilterTests(TestContext testContext)
{
    [TestMethod]
    public void Runs_after_the_filter_returned_Run()
    {
        var test = $"{testContext.FullyQualifiedTestClassName}.{testContext.TestName}";

        Assert.AreEqual(TestFilterAction.Run, CategoryFilter.Decisions[test]);
    }

    [TestMethod]
    [TestCategory(CategoryFilter.Nightly)]
    public void Is_skipped_with_the_reason_of_the_filter()
    {
        Assert.Fail("The filter skips a nightly test before it runs.");
    }

    [TestMethod]
    [TestCategory(CategoryFilter.Retired)]
    public void Is_dropped_without_a_result()
    {
        Assert.Fail("The filter drops a retired test before it runs.");
    }
}