using System.Runtime.InteropServices;
using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.MSBuild;

/// <summary>
/// The MSBuild target <c>InvokeTestingPlatform</c> starts the apphost of the test application with the variable
/// <c>DOTNET_ROOT_</c> and the architecture of the process, <c>DOTNET_ROOT_ARM64</c> on this machine, set to the
/// folder of the <c>dotnet</c> that runs MSBuild, which MSBuild names in <c>DOTNET_HOST_PATH</c>. The apphost then
/// finds the same runtime as <c>dotnet test</c>. The property <c>TestingPlatformDisableAppHostDotnetRoot</c> set
/// to <c>true</c> turns this off, and a <c>DOTNET_ROOT</c> that the project declares as a
/// <c>TestingPlatformEnvironmentVariable</c> item wins over it. This project declares that item when the
/// property <c>PinnedDotnetRoot</c> has a value. An apphost without the variable looks for the runtime in
/// <c>DOTNET_ROOT</c> and then in the global location of .NET, so the run that turns the variable off gets
/// <c>DOTNET_ROOT</c> from the example. Here <see cref="RuntimeReportTests"/> write down what their process
/// received.
/// </summary>
[TestClass]
public sealed class AppHostDotnetRootRunTests(TestContext testContext)
{
    private static readonly string RuntimeRoot =
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", "..")));

    [TestMethod]
    public async Task Target_points_the_apphost_to_the_dotnet_of_MSBuild()
    {
        var report = await ReportTheRuntimeAsync(new Dictionary<string, string>(), new Dictionary<string, string>());
        var dotnet = report.Single(static line => line.StartsWith("DOTNET_HOST_PATH=", StringComparison.Ordinal))["DOTNET_HOST_PATH=".Length..];

        Assert.IsTrue(File.Exists(dotnet));
        Assert.Contains($"{RuntimeReportTests.ArchitectureRoot}={Path.GetDirectoryName(dotnet)}", report);
    }

    [TestMethod]
    public async Task Property_turns_the_variable_off()
    {
        var report = await ReportTheRuntimeAsync(
            new Dictionary<string, string> { ["TestingPlatformDisableAppHostDotnetRoot"] = "true" },
            new Dictionary<string, string> { ["DOTNET_ROOT"] = RuntimeRoot });

        Assert.Contains($"{RuntimeReportTests.ArchitectureRoot}=", report);
        Assert.Contains($"DOTNET_ROOT={RuntimeRoot}", report);
    }

    [TestMethod]
    public async Task Declared_DOTNET_ROOT_wins_over_the_variable()
    {
        var report = await ReportTheRuntimeAsync(
            new Dictionary<string, string> { ["PinnedDotnetRoot"] = RuntimeRoot },
            new Dictionary<string, string>());

        Assert.Contains($"DOTNET_ROOT={RuntimeRoot}", report);
        Assert.Contains($"{RuntimeReportTests.ArchitectureRoot}=", report);
    }

    private async Task<string[]> ReportTheRuntimeAsync(
        IReadOnlyDictionary<string, string> properties,
        IReadOnlyDictionary<string, string> environment)
    {
        var report = Path.Combine(Directory.CreateTempSubdirectory().FullName, "runtime.txt");

        var run = await TestTargetRun.StartAsync(
            $"--filter ClassName={typeof(RuntimeReportTests).FullName}",
            properties,
            new Dictionary<string, string>(environment) { [RuntimeReportTests.Report] = report },
            testContext.CancellationToken);

        Assert.AreEqual(0, run.ExitCode, run.Output);

        return await File.ReadAllLinesAsync(report, testContext.CancellationToken);
    }
}
