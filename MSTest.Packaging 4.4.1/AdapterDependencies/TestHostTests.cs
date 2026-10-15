namespace MSTest.Packaging.AdapterDependencies;

/// <summary>
///     The test host runs the tests with MSTest.TestAdapter and without
///     Microsoft.Testing.Extensions.VSTestBridge: the bridge is not among the loaded assemblies, and the adapter
///     assembly has no reference to it.
/// </summary>
[TestClass]
public sealed class TestHostTests
{
    [TestMethod]
    public void Loads_the_adapter_without_the_VSTest_bridge()
    {
        string?[] loaded = [.. AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetName().Name)];

        Assert.Contains("MSTest.TestAdapter", loaded);
        Assert.DoesNotContain("Microsoft.Testing.Extensions.VSTestBridge", loaded);
    }

    [TestMethod]
    public void The_adapter_assembly_does_not_reference_the_VSTest_bridge()
    {
        var adapter = AppDomain.CurrentDomain.GetAssemblies()
            .Single(assembly => assembly.GetName().Name == "MSTest.TestAdapter");

        string?[] references = [.. adapter.GetReferencedAssemblies().Select(reference => reference.Name)];

        Assert.DoesNotContain("Microsoft.Testing.Extensions.VSTestBridge", references);
    }
}