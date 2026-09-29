using System.Reflection;
using System.Text.Json.Nodes;

namespace MSTest.Packaging.AdapterDependencies;

/// <summary>
/// The dependency file next to the test assembly holds the graph of the restored packages. In this graph
/// MSTest.TestAdapter depends on Microsoft.Testing.Extensions.TrxReport.Abstractions,
/// Microsoft.Testing.Extensions.Telemetry and Microsoft.TestPlatform.ObjectModel itself, and no library is
/// Microsoft.Testing.Extensions.VSTestBridge.
/// </summary>
[TestClass]
public sealed class PackageGraphTests
{
    private static JsonObject libraries = [];

    [ClassInitialize]
    public static void ReadTheDependencyFile(TestContext context)
    {
        string path = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "deps.json");
        JsonNode file = JsonNode.Parse(File.ReadAllText(path))!;
        string target = (string)file["runtimeTarget"]!["name"]!;

        libraries = file["targets"]![target]!.AsObject();
    }

    [TestMethod]
    [DataRow("Microsoft.Testing.Extensions.TrxReport.Abstractions")]
    [DataRow("Microsoft.Testing.Extensions.Telemetry")]
    [DataRow("Microsoft.TestPlatform.ObjectModel")]
    public void The_adapter_depends_on_the_package_itself(string package)
    {
        JsonNode adapter = libraries.Single(library => library.Key.StartsWith("MSTest.TestAdapter/")).Value!;

        Assert.Contains(package, adapter["dependencies"]!.AsObject().Select(dependency => dependency.Key));
    }

    [TestMethod]
    public void No_library_is_the_VSTest_bridge()
        => Assert.DoesNotContain(
            library => library.Key.StartsWith("Microsoft.Testing.Extensions.VSTestBridge/"),
            libraries);
}
