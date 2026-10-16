using MSTest.Showcase;

namespace MSTest.Execution.Timeouts;

/// <summary>
/// Each test gives itself 300 milliseconds through <c>[Timeout]</c>. That time ends while a global fixture of
/// <see cref="GlobalFixtureTimeoutTests"/> is running, which has not used up its own second. Expected outcome: both
/// tests fail, with "Global test initialize method
/// 'MSTest.Execution.Timeouts.GlobalFixtureTimeoutTests.BeforeEveryTest' was canceled" and "Global test cleanup
/// method 'MSTest.Execution.Timeouts.GlobalFixtureTimeoutTests.AfterEveryTest' was canceled".
/// </summary>
[TestClass]
public sealed class GlobalFixtureCancellationTests
{
    private const int TimeOfTheTest = 300;

    [TestMethod]
    [Explicit]
    [Timeout(TimeOfTheTest, CooperativeCancellation = true)]
    public void The_time_of_the_test_ends_during_its_global_test_initialize()
        => Assert.Fail("The test has started although its global test initialize was canceled.");

    [TestMethod]
    [Explicit]
    [Timeout(TimeOfTheTest, CooperativeCancellation = true)]
    public void The_time_of_the_test_ends_during_its_global_test_cleanup()
    {
    }
}
