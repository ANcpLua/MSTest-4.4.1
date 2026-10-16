namespace MSTest.Packaging.AdapterDependencies;

/// <summary>
/// The output folder of the build holds MSTest.TestAdapter.dll and no
/// Microsoft.Testing.Extensions.VSTestBridge.dll.
/// </summary>
[TestClass]
public sealed class OutputFolderTests
{
    [TestMethod]
    public void Holds_the_adapter_without_the_VSTest_bridge()
    {
        string?[] assemblies = [.. Directory.GetFiles(AppContext.BaseDirectory, "*.dll").Select(Path.GetFileName)];

        Assert.Contains("MSTest.TestAdapter.dll", assemblies);
        Assert.DoesNotContain("Microsoft.Testing.Extensions.VSTestBridge.dll", assemblies);
    }
}
