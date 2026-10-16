using System.Reflection;
using System.Text.Json.Nodes;

namespace MSTest.Platform.Changed.Dependencies;

/// <summary>
/// A package of the platform release line depends on Microsoft.Testing.Platform with a range: from the version
/// of the platform it is built for up to, but not including, the next major version. The assets file of the
/// restore, <c>project.assets.json</c>, holds the range that each restored package declares. The project file
/// hands its path to the test assembly with an <c>AssemblyMetadata</c> item.
/// </summary>
[TestClass]
public sealed class PlatformRangeTests
{
    private const string Platform = "Microsoft.Testing.Platform";

    private static JsonObject _packages = [];

    [ClassInitialize]
    public static void ReadTheAssetsFile(TestContext testContext)
    {
        var file = typeof(PlatformRangeTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(static metadata => metadata.Key == "ProjectAssetsFile")
            .Value!;

        _packages = JsonNode.Parse(File.ReadAllText(file))!["targets"]!.AsObject().Single().Value!.AsObject();
    }

    [TestMethod]
    [DataRow("Microsoft.Testing.Extensions.AzureDevOpsReport")]
    [DataRow("Microsoft.Testing.Extensions.CtrfReport")]
    [DataRow("Microsoft.Testing.Extensions.GitHubActionsReport")]
    [DataRow("Microsoft.Testing.Extensions.HtmlReport")]
    [DataRow("Microsoft.Testing.Extensions.JUnitReport")]
    [DataRow("Microsoft.Testing.Extensions.OpenTelemetry")]
    [DataRow("Microsoft.Testing.Extensions.PackagedApp")]
    [DataRow("Microsoft.Testing.Extensions.Retry")]
    [DataRow("Microsoft.Testing.Extensions.Telemetry")]
    [DataRow("Microsoft.Testing.Extensions.TrxReport")]
    [DataRow("Microsoft.Testing.Extensions.TrxReport.Abstractions")]
    [DataRow("Microsoft.Testing.Platform.MSBuild")]
    public void Package_accepts_the_platform_up_to_the_next_major_version(string package)
    {
        var platform = Version.Parse(VersionOf(Platform));

        var range = (string?)PackageOf(package).Value!["dependencies"]![Platform];

        Assert.AreEqual($"[{platform}, {platform.Major + 1}.0.0)", range);
    }

    private static string VersionOf(string package) => PackageOf(package).Key[(package.Length + 1)..];

    private static KeyValuePair<string, JsonNode?> PackageOf(string package) =>
        _packages.Single(restored => restored.Key.StartsWith($"{package}/", StringComparison.Ordinal));
}
