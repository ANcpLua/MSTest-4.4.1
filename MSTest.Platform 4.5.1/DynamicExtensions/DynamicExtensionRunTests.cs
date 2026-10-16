using MSTest.Platform.Terminal;

namespace MSTest.Platform.DynamicExtensions;

/// <summary>
/// <c>--enable-dynamic-extensions</c> loads the extensions that a manifest next to the test application
/// declares. A manifest is a file whose name ends with <c>.testingplatformextensions.json</c>. Each entry
/// names an assembly, relative to the manifest, and the type with the <c>AddExtensions</c> hook, and
/// <c>"enabled": false</c> keeps an entry without loading it. The platform prints what it loaded and loads
/// each assembly into a load context of its own. The manifest of this project names the two hooks of
/// <c>MSTest.Platform.Extension 4.5.1</c>, which the build copies to <c>extensions</c> in the output directory
/// and which the project does not reference. Without the option the platform reads no manifest.
/// </summary>
[TestClass]
public sealed class DynamicExtensionRunTests(TestContext testContext)
{
    private const string Extension = "MSTest.Platform.Extension 4.5.1.dll";
    private const string Hook = "MSTest.Platform.Extension.TestingPlatformBuilderHook";

    private static readonly string[] StockCounts = ["--filter", $"ClassName={typeof(StockCountTests).FullName}"];
    private static readonly string Assembly = Path.Combine(AppContext.BaseDirectory, "extensions", Extension);
    private static readonly string Manifest = Path.Combine(AppContext.BaseDirectory, "showcase.testingplatformextensions.json");

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunWithTheExtensionsOfTheManifest(TestContext context) =>
        _run = await TestApplicationRun.StartAsync(context.CancellationToken, [.. StockCounts, "--enable-dynamic-extensions"]);

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Platform_prints_the_extension_that_it_loaded()
    {
        Assert.Contains("Dynamically loaded test platform extensions (1):", _run.Output);
        Assert.Contains($"  Day shift log -> {Assembly} ({Hook}), declared in {Manifest}", _run.Output);
    }

    [TestMethod]
    public void Loaded_extension_takes_part_in_the_run()
    {
        Assert.Contains("Day shift starts", _run.Output);
        Assert.Contains("Day shift ends", _run.Output);
        Assert.AreEqual(0, _run.ExitCode);
    }

    [TestMethod]
    public void Extension_runs_in_a_load_context_of_its_own() =>
        Assert.Contains($"Day shift starts in the load context MicrosoftTestingPlatformDynamicExtension:{Assembly}", _run.Output);

    [TestMethod]
    public void Disabled_entry_is_not_loaded()
    {
        Assert.AreEqual(0, _run.ExitCode, _run.Output);
        Assert.Contains("Day shift starts", _run.Output);
        Assert.DoesNotContain("Night shift", _run.Output);
    }

    [TestMethod]
    public async Task Manifest_is_ignored_without_the_option()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, StockCounts);

        Assert.DoesNotContain("Dynamically loaded test platform extensions", run.Output);
        Assert.DoesNotContain("Day shift", run.Output);
        Assert.AreEqual(0, run.ExitCode);
    }
}
