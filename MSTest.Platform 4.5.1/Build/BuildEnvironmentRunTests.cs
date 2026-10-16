using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MSTest.Showcase;

namespace MSTest.Platform.Build;

[TestClass]
public sealed class BuildEnvironmentRunTests(TestContext testContext)
{
    [TestMethod]
    [Timeout(60_000, CooperativeCancellation = true)]
    public async Task MSBuild_test_target_preserves_the_inherited_environment()
    {
        var sentinel = Guid.NewGuid().ToString("N");
        var start = new ProcessStartInfo("dotnet", ["build", ProjectPath(), "--no-restore", "-t:Test", "-m:2",
            "-p:TestingPlatformCaptureOutput=false", "-p:TestingPlatformCommandLineArguments=--output Detailed"])
        {
            WorkingDirectory = Path.GetDirectoryName(ProjectPath())!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Environment = { ["MSTEST_SHOWCASE_BUILD_SENTINEL"] = sentinel, ["TESTINGPLATFORM_UI_LANGUAGE"] = "en-us" }
        };
        await using var child = ChildProcess.Start(start, testContext.CancellationToken);
        var output = child.Process.StandardOutput.ReadToEndAsync(testContext.CancellationToken);
        var error = child.Process.StandardError.ReadToEndAsync(testContext.CancellationToken);
        await child.Process.WaitForExitAsync(testContext.CancellationToken);
        var text = await output + await error;
        Assert.AreEqual(0, child.Process.ExitCode, text);
        Assert.Contains("succeeded: 3", text);
        Assert.Contains("failed: 0", text);
        var record = Assert.ContainsSingle(text.Split('\n').Select(static line => line.Trim())
            .Where(static line => line.StartsWith("{\"inheritedBuildSentinel\":", StringComparison.Ordinal)));
        using var payload = JsonDocument.Parse(record);
        Assert.AreEqual(sentinel, payload.RootElement.GetProperty("inheritedBuildSentinel").GetString());
    }

    private static string ProjectPath([CallerFilePath] string source = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(source)!, "..", "..",
            "MSTest.Platform.TestTarget 4.5.1", "MSTest.Platform.TestTarget 4.5.1.csproj"));
}
