namespace MSTest.Retry.DataDriven;

/// <summary>
/// What the test host reports about a run of <see cref="InconclusiveRowTests"/>. An inconclusive result is
/// printed as skipped. A folded inconclusive row runs again in the second attempt because the other row of
/// its first attempt failed or timed out, and no third attempt follows although retries are left. A test
/// whose last attempt has a skipped row does not count as flaky.
/// </summary>
[TestClass]
public sealed class InconclusiveRowRunTests
{
    private const string Failed = nameof(InconclusiveRowTests.Rows_run_again_while_one_of_them_failed);
    private const string TimedOut = nameof(InconclusiveRowTests.Rows_run_again_while_one_of_them_timed_out);
    private const string Unfolded = nameof(InconclusiveRowTests.Unfolded_rows_are_retried_on_their_own);
    private const string OnItsOwn = nameof(InconclusiveRowTests.An_inconclusive_result_on_its_own_ends_the_retries);
    private const string Inconclusive = " (\"inconclusive\")";
    private const string Unstable = " (\"unstable\")";

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheRows(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(InconclusiveRowTests).FullName}",
            "--output", "Detailed");

    [TestMethod]
    public void Folded_rows_all_run_again_after_one_of_them_failed()
    {
        Assert.AreSequenceEqual(["skipped (try 1)", "skipped (try 2)"], _run.AttemptsOf(Failed + Inconclusive));
        Assert.AreSequenceEqual(["failed (try 1)", "passed (try 2)"], _run.AttemptsOf(Failed + Unstable));
    }

    [TestMethod]
    public void Folded_rows_all_run_again_after_one_of_them_timed_out()
    {
        Assert.AreSequenceEqual(["skipped (try 1)", "skipped (try 2)"], _run.AttemptsOf(TimedOut + Inconclusive));
        Assert.AreSequenceEqual(["failed (try 1)", "passed (try 2)"], _run.AttemptsOf(TimedOut + Unstable));
        Assert.Contains($"Test '{TimedOut}' timed out after 1000ms", _run.Output);
    }

    [TestMethod]
    public void Unfolded_inconclusive_row_has_a_single_attempt()
    {
        Assert.IsEmpty(_run.AttemptsOf(Unfolded + Inconclusive));
        Assert.Contains($"skipped {Unfolded}{Inconclusive}", _run.Output);
        Assert.AreSequenceEqual(["failed (try 1)", "passed (try 2)"], _run.AttemptsOf(Unfolded + Unstable));
    }

    [TestMethod]
    public void Inconclusive_result_on_its_own_is_the_last_attempt() =>
        Assert.AreSequenceEqual(["failed (try 1)", "skipped (try 2)"], _run.AttemptsOf(OnItsOwn));

    [TestMethod]
    public void Summary_counts_only_the_unfolded_row_as_flaky()
    {
        Assert.Contains("  total: 7", _run.Output);
        Assert.Contains("  succeeded: 3", _run.Output);
        Assert.Contains("  skipped: 4", _run.Output);
        Assert.Contains("  flaky: 1 (passed after retry)", _run.Output);
        Assert.Contains("  retried: 4 test(s), 6 extra run(s)", _run.Output);
        Assert.Contains($"  {Unfolded}{Unstable} failed -> passed (2 attempts)", _run.Output);
        Assert.AreEqual(0, _run.ExitCode);
    }
}
