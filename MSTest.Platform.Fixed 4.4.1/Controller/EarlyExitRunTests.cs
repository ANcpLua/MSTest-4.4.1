using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Controller;

/// <summary>
///     When the test host ends before it connects to the controller, the controller prints the exit code of the test
///     host with the elapsed time and ends the run with exit code 1. Here
///     <see cref="TestHostStartupHooks" /> gives the test host of a run of <see cref="ShelfTests" /> a startup hook that
///     does not exist, so the runtime aborts the test host before its <c>Main</c> with exit code 134 on macOS, and the
///     output reports the early exit while the configured connection timeout is 5 seconds.
/// </summary>
[TestClass]
public sealed class EarlyExitRunTests
{
    private const int GenericFailure = 1;

    private static readonly string MissingHook = Path.Combine(Path.GetTempPath(), "showcase-missing-startup-hook.dll");

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task StartATestHostThatEndsAtOnce(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            new Dictionary<string, string>
            {
                [TestHostStartupHooks.Variable] = MissingHook,
                ["TESTINGPLATFORM_DEFAULT_HANG_TIMEOUT"] = "5s"
            },
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(ShelfTests).FullName}");
    }

    [ClassCleanup]
    public static void DeleteTheResults()
    {
        _run.Dispose();
    }

    [TestMethod]
    public void Test_host_ends_before_its_Main()
    {
        Assert.Contains($"Startup hook assembly '{MissingHook}' failed to load.", _run.Error);
    }

    [TestMethod]
    public void Output_names_the_elapsed_time_and_the_exit_code_of_the_test_host()
    {
        Assert.MatchesRegex(
            @"The test host process did not connect to the controller's named pipe after '[0-9]+(?:\.[0-9]+)?' seconds\. The test host process exited with code '134'\.",
            _run.Output);
    }

    [TestMethod]
    public void Run_ends_with_exit_code_1()
    {
        Assert.AreEqual(GenericFailure, _run.ExitCode);
    }
}