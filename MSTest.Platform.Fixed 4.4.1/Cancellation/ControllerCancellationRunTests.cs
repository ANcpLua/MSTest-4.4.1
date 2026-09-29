using System.Diagnostics;
using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Cancellation;

/// <summary>
/// A run with <c>--report-trx</c> has two processes: the controller, which the command starts, and the test
/// host, which the controller starts. Ctrl+C in the controller, the signal SIGINT on macOS and Linux, cancels the
/// tests in the test host, whose <c>TestContext.CancellationToken</c> is cancelled, and the controller waits for
/// the test host to clean up and end. It ends the test host only when the cleanup takes longer than the bounded
/// period, 45 seconds by default. The run ends as aborted with exit code 3 in both cases. Here the example sends
/// SIGINT with the command <c>kill -INT</c> to the controller of a run of <see cref="HandoverTests"/> once the
/// test waits.
/// </summary>
[TestClass]
public sealed class ControllerCancellationRunTests(TestContext testContext)
{
    private const int TestSessionAborted = 3;

    [TestMethod]
    public async Task Test_host_cleans_up_before_the_run_ends()
    {
        var depot = Directory.CreateTempSubdirectory();

        using var run = await CancelAsync(nameof(HandoverTests.Hands_over_before_stopping), depot);

        Assert.IsTrue(File.Exists(Path.Combine(depot.FullName, HandoverTests.HandedOver)));
        Assert.Contains("Canceling the test session...", run.Output);
        Assert.Contains("Test run summary: Aborted", run.Output);
        Assert.DoesNotContain("didn't exit gracefully", run.Output);
        Assert.AreEqual(TestSessionAborted, run.ExitCode);
    }

    [TestMethod]
    public async Task Test_host_ends_once_its_cleanup_outlasts_the_bounded_period()
    {
        var depot = Directory.CreateTempSubdirectory();
        var clock = Stopwatch.StartNew();

        using var run = await CancelAsync(nameof(HandoverTests.Stalls_while_stopping), depot);

        Assert.IsGreaterThanOrEqualTo(TimeSpan.FromSeconds(45), clock.Elapsed);
        Assert.IsLessThan(TimeSpan.FromMinutes(2), clock.Elapsed);
        Assert.IsFalse(File.Exists(Path.Combine(depot.FullName, HandoverTests.HandedOver)));
        Assert.Contains("Test application process didn't exit gracefully, exit code is '137'", run.Output);
        Assert.AreEqual(TestSessionAborted, run.ExitCode);
    }

    private Task<TestApplicationRun> CancelAsync(string test, DirectoryInfo depot) =>
        TestApplicationRun.StartAsync(
            Environment.ProcessPath!,
            Directory.CreateTempSubdirectory(),
            new Dictionary<string, string> { [HandoverTests.Depot] = depot.FullName },
            async controller =>
            {
                while (!File.Exists(Path.Combine(depot.FullName, HandoverTests.Waiting)))
                {
                    await Task.Delay(100, testContext.CancellationToken);
                }

                using var kill = Process.Start("kill", ["-INT", controller.ToString(System.Globalization.CultureInfo.InvariantCulture)]);
                await kill.WaitForExitAsync(testContext.CancellationToken);
            },
            testContext.CancellationToken,
            "--filter", $"FullyQualifiedName={typeof(HandoverTests).FullName}.{test}",
            "--report-trx");
}
