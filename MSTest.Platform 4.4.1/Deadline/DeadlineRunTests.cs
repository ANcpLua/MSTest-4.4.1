using System.Xml.Linq;

namespace MSTest.Platform.Deadline;

/// <summary>
/// A run of <see cref="QueueTests"/> with a deadline. <c>TESTINGPLATFORM_DEADLINE</c> is the time at which
/// the run is cancelled from outside, as an ISO 8601 time with its offset. The platform asks the test
/// framework to stop before that time, by the margin of <c>TESTINGPLATFORM_DEADLINE_STOP_MARGIN</c>, which
/// is 60 seconds unless the variable names another number of seconds. The test that runs at that moment
/// finishes, no further test starts, the reports are written and the run ends with exit code 15. The feature
/// is a prototype and needs a test framework that can stop a run, as MSTest can.
/// </summary>
[TestClass]
public sealed class DeadlineRunTests(TestContext testContext)
{
    private const string Report = "queue.trx";
    private const int TestExecutionStoppedAtDeadline = 15;

    private static readonly string[] Queue =
        ["--filter", $"ClassName={typeof(QueueTests).FullName}", "--output", "detailed", "--report-trx", "--report-trx-filename", Report];

    private static TestApplicationRun _stopped = null!;

    [ClassInitialize]
    public static async Task RunTheQueueWithADeadline(TestContext context) =>
        _stopped = await RunAsync(DateTimeOffset.UtcNow.AddSeconds(5), stopMargin: "0", context.CancellationToken);

    [ClassCleanup]
    public static void DeleteTheResults() => _stopped.Dispose();

    [TestMethod]
    public void Platform_announces_the_stop() =>
        Assert.Contains(
            "Deadline approaching: gracefully stopping the test run so reports can be finalized before the CI hard-cancel.",
            _stopped.Output);

    [TestMethod]
    public void Test_that_was_running_finishes_and_no_further_test_starts()
    {
        Assert.HasCount(1, _stopped.ResultLines);
        Assert.AreSequenceEqual(["total: 1", "failed: 0", "succeeded: 1", "skipped: 0"], _stopped.LinesBelow("Test run summary:").Take(4));
    }

    [TestMethod]
    public void Run_ends_with_exit_code_15() => Assert.AreEqual(TestExecutionStoppedAtDeadline, _stopped.ExitCode);

    [TestMethod]
    public void Report_of_the_stopped_run_is_written()
    {
        XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var report = XDocument.Load(_stopped.PathOf(Report));

        Assert.AreSequenceEqual(["Passed"], report.Descendants(trx + "UnitTestResult").Select(static result => result.Attribute("outcome")!.Value));
    }

    [TestMethod]
    public async Task Margin_moves_the_stop_ahead_of_the_deadline()
    {
        using var run = await RunAsync(DateTimeOffset.UtcNow.AddSeconds(65), stopMargin: "60", testContext.CancellationToken);

        Assert.HasCount(1, run.ResultLines);
        Assert.AreEqual(TestExecutionStoppedAtDeadline, run.ExitCode);
    }

    [TestMethod]
    public async Task Run_without_a_deadline_runs_every_test()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, Queue);

        Assert.HasCount(3, run.ResultLines);
        Assert.AreEqual(0, run.ExitCode);
    }

    private static Task<TestApplicationRun> RunAsync(DateTimeOffset deadline, string stopMargin, CancellationToken cancellationToken) =>
        TestApplicationRun.StartAsync(
            new Dictionary<string, string>
            {
                [QueueTests.Deadline] = deadline.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"),
                [QueueTests.StopMargin] = stopMargin,
            },
            cancellationToken,
            Queue);
}
