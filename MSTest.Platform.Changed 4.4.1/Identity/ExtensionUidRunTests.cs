namespace MSTest.Platform.Changed.Identity;

/// <summary>
/// A built-in extension of the platform has a fixed identifier, its UID. <c>--info</c> lists every provider of
/// command-line options under its UID, followed by its name, and an error about the options of a provider
/// names the UID as well. The providers of the Azure DevOps, CTRF, GitHub Actions, HTML and JUnit reports and
/// of the retries are part of this application because the project file enables their extensions.
/// </summary>
[TestClass]
public sealed class ExtensionUidRunTests(TestContext testContext)
{
    private const int InvalidCommandLine = 5;

    private static TestApplicationRun _information = null!;

    [ClassInitialize]
    public static async Task AskForTheInformation(TestContext testContext) =>
        _information = await TestApplicationRun.StartAsync(testContext.CancellationToken, "--info");

    [ClassCleanup]
    public static void DeleteTheResults() => _information.Dispose();

    [TestMethod]
    [DataRow("PlatformCommandLineProvider", "Platform command line provider")]
    [DataRow("TerminalTestReporterCommandLineOptionsProvider", "Terminal test reporter")]
    [DataRow("AzureDevOpsCommandLineProvider", "Azure DevOps report generator")]
    [DataRow("CtrfReportGeneratorCommandLine", "CTRF report generator")]
    [DataRow("GitHubActionsCommandLineProvider", "GitHub Actions report generator")]
    [DataRow("HtmlReportGeneratorCommandLine", "HTML report generator")]
    [DataRow("JUnitReportGeneratorCommandLine", "JUnit XML report generator")]
    [DataRow("MSBuildCommandLineProvider", "MSBuild integration")]
    [DataRow("RetryCommandLineOptionsProvider", "Retry failed tests")]
    [DataRow("TrxReportGeneratorCommandLine", "TRX report generator")]
    public void Information_lists_the_provider_under_its_UID(string uid, string name) =>
        Assert.Contains($"  {uid}{Environment.NewLine}    Name: {name}{Environment.NewLine}", _information.Output);

    [TestMethod]
    public async Task Error_about_an_option_names_the_UID_of_its_provider()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, "--report-azdo-groups", "on");

        Assert.AreEqual(InvalidCommandLine, run.ExitCode);
        Assert.Contains(
            "Invalid configuration for provider 'Azure DevOps report generator' (UID: AzureDevOpsCommandLineProvider). "
            + "Error: '--report-azdo-groups' requires '--report-azdo' to be enabled",
            run.Output);
    }
}
