using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.GeneratedPath;

/// <summary>
/// <c>EnableMSTestSourceGeneration</c> in the project file adds the MSTest source generator to a project that
/// is not published with Native AOT. The generator writes the registry
/// <see cref="MSTestReflectionMetadata"/> into the test assembly, with an entry for each test class that it
/// supports, and a module initializer that hands the registry to MSTest.
/// <c>EmitCompilerGeneratedFiles</c> keeps the generated source below <c>artifacts/obj</c>.
/// </summary>
[TestClass]
public sealed class GeneratedRegistryTests
{
    [TestMethod]
    public void Registry_names_the_test_assembly()
        => Assert.AreEqual(MSTestReflectionMetadata.AssemblyName, typeof(GeneratedRegistryTests).Assembly.GetName().Name);

    [TestMethod]
    public void Registry_has_one_entry_for_the_test_class()
        => Assert.ContainsSingle(static entry => entry.Type == typeof(GeneratedRegistryTests), MSTestReflectionMetadata.TestClasses);

    [TestMethod]
    public void Entry_lists_the_test_methods_of_the_class()
    {
        var entry = MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(GeneratedRegistryTests));

        Assert.AreSequenceEqual(
            [
                nameof(Registry_names_the_test_assembly),
                nameof(Registry_has_one_entry_for_the_test_class),
                nameof(Entry_lists_the_test_methods_of_the_class),
            ],
            entry.Methods.Select(static method => method.Name));
    }
}
