using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Orchestration;

/// <summary>
/// An <c>ITestHostOrchestratorApplicationLifetime</c> gets its <c>AfterRunAsync</c> call after a failed or a
/// cancelled orchestration as well, so it can release what it acquired in <c>BeforeRunAsync</c>. After a failure
/// the platform passes exit code 1. Here <see cref="RunLedger"/>, which
/// <c>Program.Main</c> registers with <c>AddTestHostOrchestratorApplicationLifetime</c>, reports both calls of
/// two runs with <c>--retry-failed-tests 1</c>, whose orchestrator starts the test hosts. The Retry extension
/// refuses to run while hot reload is on, which the variable <c>TESTINGPLATFORM_HOTRELOAD_ENABLED</c> set to
/// <c>1</c> turns on, and the application ends with that unhandled exception after the ledger entry is closed.
/// </summary>
[TestClass]
public sealed class OrchestratorLifetimeRunTests(TestContext testContext)
{
    [TestMethod]
    public async Task Ledger_entry_is_closed_after_a_normal_run()
    {
        using var run = await RunTheShelfWithRetryAsync(new Dictionary<string, string>());

        Assert.Contains("Ledger entry opened.", run.Output);
        Assert.Contains("Ledger entry closed with exit code 0.", run.Output);
        Assert.AreEqual(0, run.ExitCode);
    }

    [TestMethod]
    public async Task Ledger_entry_is_closed_when_the_orchestrator_fails()
    {
        using var run = await RunTheShelfWithRetryAsync(new Dictionary<string, string> { ["TESTINGPLATFORM_HOTRELOAD_ENABLED"] = "1" });

        Assert.Contains("Ledger entry opened.", run.Output);
        Assert.Contains("Ledger entry closed with exit code 1.", run.Output);
        Assert.Contains("System.InvalidOperationException: Retry failed tests feature is not supported in hot reload mode", run.Error);
        Assert.DoesNotContain("total:", run.Output);
    }

    private Task<TestApplicationRun> RunTheShelfWithRetryAsync(IReadOnlyDictionary<string, string> environment) =>
        TestApplicationRun.StartAsync(
            environment,
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(ShelfTests).FullName}",
            "--retry-failed-tests", "1");
}
