using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.CommandLine;

/// <summary>
/// The options of the GitHub Actions report, such as <c>--report-gh-step-summary</c>, take effect only with
/// <c>--report-gh</c>. A run with such an option and without <c>--report-gh</c> stops before any test with an
/// error and exit code 5, the code of an invalid command line. The options need
/// <c>EnableMicrosoftTestingExtensionsGitHubActionsReport</c> in the project file.
/// </summary>
[TestClass]
public sealed class GitHubSubOptionRunTests(TestContext testContext)
{
    private const int InvalidCommandLine = 5;

    [TestMethod]
    public async Task Sub_option_without_report_gh_is_rejected()
    {
        using var run = await RunTheShelfAsync("--report-gh-step-summary", "on");

        Assert.Contains(
            "Invalid configuration for provider 'GitHub Actions report generator' (UID: GitHubActionsCommandLineProvider). Error: The GitHub Actions report sub-options require '--report-gh' to be specified.",
            run.Output);
        Assert.DoesNotContain("total:", run.Output);
        Assert.AreEqual(InvalidCommandLine, run.ExitCode);
    }

    [TestMethod]
    public async Task Sub_option_with_report_gh_is_accepted()
    {
        using var run = await RunTheShelfAsync("--report-gh", "--report-gh-step-summary", "on");

        Assert.Contains("  succeeded: 1", run.Output);
        Assert.AreEqual(0, run.ExitCode);
    }

    private Task<TestApplicationRun> RunTheShelfAsync(params string[] options) =>
        TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            ["--filter", $"ClassName={typeof(ShelfTests).FullName}", .. options]);
}
