using System.Reflection;

namespace MSTest.Platform.Fixed.WebAssembly;

/// <summary>
/// A test application for browser-wasm writes its TRX report with <c>--report-trx</c> and runs with the Azure
/// DevOps extension, which creates its HTTP client when the test host builds its extensions. Its output names
/// every file that a test or the run produces, the TRX report as well, with the display name of the file and its
/// path in the virtual file system of the browser-wasm runtime, which is not a path on the disk of the machine.
/// Here the example starts MSTest.Platform.Fixed.Browser 4.4.1, whose one test attaches a file with
/// <c>TestContext.AddResultFile</c>, with <c>dotnet run</c> under the JavaScript host bun. The project references
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
    public static async Task RunTheBrowserApplication(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            "dotnet",
            Directory.CreateTempSubdirectory(),
            new Dictionary<string, string>(),
            testContext.CancellationToken,
            "run", "--no-build", "--project", Project, "--",
            "--report-trx", "--report-trx-filename", Trx,
            "--report-azdo");

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
        Assert.AreEqual(0, _run.ExitCode);
    }
}
