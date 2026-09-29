using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Controller;

/// <summary>
/// When the test host ends before it connects to the controller, the controller prints the exit code of the test
/// host and ends the run with exit code 1. If the test host has already ended when the controller checks right
/// after starting it, this happens at once and the message says 'after '0' seconds'. Otherwise the controller
/// waits for the named-pipe connection timeout, 300 seconds by default and set with
/// <c>TESTINGPLATFORM_DEFAULT_HANG_TIMEOUT</c>, and the message also names that timeout. Here
/// <see cref="TestHostStartupHooks"/> gives the test host of a run of <see cref="ShelfTests"/> a startup hook that
/// does not exist, so the runtime aborts the test host before its <c>Main</c> with exit code 134 on macOS, and the
/// output shows the second case with a timeout of 5 seconds.
/// </summary>
[TestClass]
public sealed class EarlyExitRunTests
{
    private const int GenericFailure = 1;

    private static readonly string MissingHook = Path.Combine(Path.GetTempPath(), "showcase-missing-startup-hook.dll");

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task StartATestHostThatEndsAtOnce(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            new Dictionary<string, string>
            {
                [TestHostStartupHooks.Variable] = MissingHook,
                ["TESTINGPLATFORM_DEFAULT_HANG_TIMEOUT"] = "5s",
            },
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(ShelfTests).FullName}");

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Test_host_ends_before_its_Main() =>
        Assert.Contains($"Startup hook assembly '{MissingHook}' failed to load.", _run.Error);

    [TestMethod]
    public void Output_names_the_timeout_and_the_exit_code_of_the_test_host() =>
        Assert.Contains(
            "The test host process did not connect to the controller's named pipe after '5' seconds. The configured connection timeout was '5' seconds. The test host process exited with code '134'.",
            _run.Output);

    [TestMethod]
    public void Run_ends_with_exit_code_1() => Assert.AreEqual(GenericFailure, _run.ExitCode);
}
