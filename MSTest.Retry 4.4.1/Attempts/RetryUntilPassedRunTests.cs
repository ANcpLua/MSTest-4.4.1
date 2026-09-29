namespace MSTest.Retry.Attempts;

/// <summary>
/// What the test host reports about a run of <see cref="RetryUntilPassedTests"/>: the first run and, in the
/// order in which <see cref="RetryUntilPassedAttribute"/> added them to its <see cref="RetryResult"/>, the two
/// attempts of the policy. The last attempt passed after a failed one, so the test counts as flaky.
/// </summary>
[TestClass]
public sealed class RetryUntilPassedRunTests(TestContext testContext)
{
    private const string Test = nameof(RetryUntilPassedTests.Passes_after_a_failed_and_an_inconclusive_attempt);

    [TestMethod]
    public async Task Test_host_receives_every_attempt_of_the_policy_in_order()
    {
        var run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(RetryUntilPassedTests).FullName}",
            "--output", "Detailed");

        Assert.AreSequenceEqual(["failed (try 1)", "skipped (try 2)", "passed (try 3)"], run.AttemptsOf(Test));
        Assert.Contains("  flaky: 1 (passed after retry)", run.Output);
        Assert.Contains("  retried: 1 test(s), 2 extra run(s)", run.Output);
        Assert.AreEqual(0, run.ExitCode);
    }
}
