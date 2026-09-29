namespace MSTest.Platform.Deadline;

/// <summary>
/// A run of <see cref="StuckTests"/> with a deadline and <c>--hangdump</c>. The platform asks for the stop
/// first, and the test does not end. <c>TESTINGPLATFORM_DEADLINE_DUMP_MARGIN</c> is the time before the
/// deadline at which the hang dump extension steps in, 30 seconds unless the variable names another number
/// of seconds: it lists the tests that are still running, writes that list and the dump to the results
/// directory and ends the test host, so the run ends with exit code 7. <c>--hangdump-type None</c> leaves out
/// the dump. <c>--hangdump</c> needs <c>EnableMicrosoftTestingExtensionsHangDump</c> in the project file.
/// </summary>
[TestClass]
public sealed class DeadlineDumpRunTests
{
    private const string DumpMargin = "TESTINGPLATFORM_DEADLINE_DUMP_MARGIN";
    private const string Stuck = nameof(StuckTests.Waits_for_a_lock_that_nobody_releases);
    private const int TestHostProcessExitedNonGracefully = 7;

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheStuckTestWithADeadline(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            new Dictionary<string, string>
            {
                [QueueTests.Deadline] = DateTimeOffset.UtcNow.AddSeconds(9).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"),
                [QueueTests.StopMargin] = "4",
                [DumpMargin] = "2",
            },
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(StuckTests).FullName}",
            "--hangdump", "--hangdump-type", "None");

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Stop_is_requested_before_the_dump()
    {
        var stop = _run.Output.IndexOf("Deadline approaching: gracefully stopping the test run", StringComparison.Ordinal);
        var dump = _run.Output.IndexOf("CI deadline approaching: taking a hang dump before the run is hard-cancelled.", StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, stop);
        Assert.IsGreaterThan(stop, dump);
    }

    [TestMethod]
    public void Extension_lists_the_test_that_is_still_running() =>
        Assert.MatchesRegex($@"\[\d\d:\d\d:\d\d\] {Stuck}", _run.Output);

    [TestMethod]
    public void List_of_the_running_tests_is_an_artifact_of_the_run()
    {
        var list = Assert.ContainsSingle(_run.Results.GetFiles("*_hang.log"));

        Assert.MatchesRegex($@"^\[\d\d:\d\d:\d\d\] {Stuck}\s*$", File.ReadAllText(list.FullName));
        Assert.IsEmpty(_run.Results.GetFiles("*.dmp"));
    }

    [TestMethod]
    public void Run_ends_with_the_exit_code_of_a_test_host_that_was_ended() =>
        Assert.AreEqual(TestHostProcessExitedNonGracefully, _run.ExitCode);
}
