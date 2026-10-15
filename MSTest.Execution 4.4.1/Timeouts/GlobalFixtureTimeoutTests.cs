using MSTest.Showcase;

namespace MSTest.Execution.Timeouts;

/// <summary>
///     testconfig.json gives the global test fixtures one second, through <c>mstest:timeout:globalTestInitialize</c>
///     and <c>mstest:timeout:globalTestCleanup</c>, and the test fixtures two seconds, through
///     <c>mstest:timeout:testInitialize</c> and <c>mstest:timeout:testCleanup</c>. The two global fixtures here take
///     three seconds for the tests that are named in them and no time for any other test of the assembly. Expected
///     outcome: both tests fail, with "Global test initialize method
///     'MSTest.Execution.Timeouts.GlobalFixtureTimeoutTests.BeforeEveryTest' timed out after 1000ms" and "Global test
///     cleanup method 'MSTest.Execution.Timeouts.GlobalFixtureTimeoutTests.AfterEveryTest' timed out after 1000ms".
///     The same two messages end with "600ms" and "700ms" when <c>--config-file</c> names
///     Timeouts/fallback.testconfig.json, which has <c>testInitialize</c> and <c>testCleanup</c> only, and with "800ms"
///     and "900ms" when <c>--config-file</c> names Timeouts/no-mstest.testconfig.json and <c>--settings</c> names
///     Timeouts/global-fixtures.runsettings, which has <c>GlobalTestInitializeTimeout</c> and
///     <c>GlobalTestCleanupTimeout</c>.
/// </summary>
[TestClass]
public sealed class GlobalFixtureTimeoutTests
{
    private static readonly TimeSpan LongerThanEveryTimeout = TimeSpan.FromSeconds(3);

    // Invoked by MSTest through its global-fixture attribute.
    // ReSharper disable once UnusedMember.Global
    [GlobalTestInitialize]
    public static Task BeforeEveryTest(TestContext testContext)
    {
        return testContext.TestName
            is nameof(The_global_test_initialize_takes_too_long)
            or nameof(GlobalFixtureCancellationTests.The_time_of_the_test_ends_during_its_global_test_initialize)
            ? Task.Delay(LongerThanEveryTimeout, testContext.CancellationToken)
            : Task.CompletedTask;
    }

    // Invoked by MSTest through its global-fixture attribute.
    // ReSharper disable once UnusedMember.Global
    [GlobalTestCleanup]
    public static Task AfterEveryTest(TestContext testContext)
    {
        return testContext.TestName
            is nameof(The_global_test_cleanup_takes_too_long)
            or nameof(GlobalFixtureCancellationTests.The_time_of_the_test_ends_during_its_global_test_cleanup)
            ? Task.Delay(LongerThanEveryTimeout, testContext.CancellationToken)
            : Task.CompletedTask;
    }

    [TestMethod]
    [Explicit]
    public void The_global_test_initialize_takes_too_long()
    {
        Assert.Fail("The test has started although its global test initialize timed out.");
    }

    [TestMethod]
    [Explicit]
    public void The_global_test_cleanup_takes_too_long()
    {
    }
}