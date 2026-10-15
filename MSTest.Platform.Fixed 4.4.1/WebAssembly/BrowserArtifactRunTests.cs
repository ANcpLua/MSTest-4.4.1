using System.Diagnostics;
using System.Reflection;
using MSTest.Showcase;

namespace MSTest.Platform.Fixed.WebAssembly;

/// <summary>
/// A test application for browser-wasm writes its TRX report with <c>--report-trx</c> and runs with the Azure
/// DevOps extension, which creates its HTTP client when the test host builds its extensions. Its output names
/// every file that a test or the run produces, the TRX report as well, with the display name of the file and its
/// path in the virtual file system of the browser-wasm runtime, which is not a path on the disk of the machine.
/// Here the example starts MSTest.Platform.Fixed.Browser 4.4.1, whose one test attaches a file with
/// <c>TestContext.AddResultFile</c>, by publishing its bundle and running <c>wwwroot/runtests.mjs</c> under Node.
/// Publishing is part of this fixture's setup; no browser DLL is sent to the desktop test driver. The project references
/// the packages Microsoft.Testing.Extensions.TrxReport and Microsoft.Testing.Extensions.AzureDevOpsReport.
/// </summary>
[TestClass]
public sealed class BrowserArtifactRunTests
{
    private const string Trx = "notes.trx";

    private static readonly string Project = typeof(BrowserArtifactRunTests).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(static attribute => attribute.Key == "BrowserProjectPath").Value!;

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheBrowserApplication(TestContext testContext)
    {
        var results = Directory.CreateTempSubdirectory();
        try
        {
            var publish = Path.Combine(results.FullName, "publish");
            var start = new ProcessStartInfo("dotnet",
                ["publish", Project, "--configuration", "Debug", "--output", publish, "--nologo", "--disable-build-servers"])
            {
                WorkingDirectory = Path.GetDirectoryName(Project)!,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            await using (var child = ChildProcess.Start(start, testContext.CancellationToken))
            {
                var output = child.Process.StandardOutput.ReadToEndAsync(testContext.CancellationToken);
                var error = child.Process.StandardError.ReadToEndAsync(testContext.CancellationToken);
                await child.Process.WaitForExitAsync(testContext.CancellationToken);
                Assert.AreEqual(0, child.Process.ExitCode, $"Browser publish failed:\n{await output}\n{await error}");
            }

            _run = await TestApplicationRun.StartAsync(
                "node",
                results,
                new Dictionary<string, string>(),
                testContext.CancellationToken,
                Path.Combine(publish, "wwwroot", "runtests.mjs"),
                "--minimum-expected-tests", "1",
                "--report-trx", "--report-trx-filename", Trx,
                "--report-azdo");
        }
        catch
        {
            results.Delete(recursive: true);
            throw;
        }
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Output_names_the_TRX_report_with_its_virtual_path()
    {
        Assert.Contains($"TRX Report: {_run.PathOf(Trx)}", _run.Output);
        Assert.IsFalse(File.Exists(_run.PathOf(Trx)));
    }

    [TestMethod]
    public void Output_names_the_file_that_the_test_attached() =>
        Assert.ContainsSingle(
            _run.Output.Split('\n').Where(static line => line.StartsWith("MSTestAdapterV2: ", StringComparison.Ordinal) && line.EndsWith("/signed-delivery-note.txt", StringComparison.Ordinal)));

    [TestMethod]
    public void Run_passes()
    {
        Assert.Contains("Test run summary: Passed!", _run.Output);
        Assert.Contains("  total: 1", _run.Output);
        Assert.Contains("  succeeded: 1", _run.Output);
        Assert.Contains("  failed: 0", _run.Output);
        Assert.AreEqual(0, _run.ExitCode);
    }
}
