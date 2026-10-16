namespace MSTest.Execution.Timeouts;

/// <summary>
/// <c>mstest:timeout:globalTestInitialize</c> and <c>mstest:timeout:globalTestCleanup</c> limit the global test
/// fixtures only. The <c>[TestInitialize]</c> and the <c>[TestCleanup]</c> of this class take longer than the one
/// second that testconfig.json gives the global fixtures and less than the two seconds that it gives the test
/// fixtures, and the test passes.
/// </summary>
[TestClass]
public sealed class TestFixtureTests(TestContext testContext)
{
    private static readonly TimeSpan LongerThanTheGlobalTimeouts = TimeSpan.FromMilliseconds(1_200);

    private bool _initialized;

    [TestInitialize]
    public async Task Initialize()
    {
        await Task.Delay(LongerThanTheGlobalTimeouts, testContext.CancellationToken);
        _initialized = true;
    }

    [TestCleanup]
    public Task Cleanup() => Task.Delay(LongerThanTheGlobalTimeouts, testContext.CancellationToken);

    [TestMethod]
    public void The_test_initialize_has_run_to_its_end() => Assert.IsTrue(_initialized);
}
