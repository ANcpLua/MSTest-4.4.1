using System.Runtime.CompilerServices;
using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.Diagnostics;

/// <summary>
/// AOTSG0004 reports a generic method of a test class: the generator writes no entry and no delegate for a
/// method whose type arguments are known at run time only. Under the JIT compiler the test still runs,
/// because MSTest finds it by looking the methods up and calls it through reflection. A native executable
/// has no code for that call with a value type, as the Native AOT project shows. A test that is not generic
/// is called by the registry.
/// </summary>
[TestClass]
public sealed class GenericTestMethodTests
{
    private const string Registry = "MSTest.SourceGenerated.MSTestReflectionMetadata";
    private const string Reflection = "System.Reflection";

    private static TestClassReflectionInfo Class
        => MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(GenericTestMethodTests));

    // AOTSG0004
    [TestMethod]
    [DataRow(1)]
    [DataRow("one")]
    public void Generic_test_is_called_through_reflection<T>(T value)
    {
        Assert.IsNotNull(value);
        Assert.DoesNotContain(nameof(Generic_test_is_called_through_reflection), Class.Methods.Select(static method => method.Name));
        Assert.Contains(Reflection, Environment.StackTrace);
        Assert.DoesNotContain(Registry, Environment.StackTrace);
    }

    [TestMethod]
    [DataRow(1)]
    public void Test_with_an_integer_is_called_by_the_registry(int value) => AssertCalledByTheRegistry(value);

    [TestMethod]
    [DataRow("one")]
    public void Test_with_a_string_is_called_by_the_registry(string value) => AssertCalledByTheRegistry(value);

    [TestMethod]
    public void Descriptors_of_the_class_are_incomplete() => Assert.IsFalse(Class.AreGeneratedDescriptorsComplete);

    private static void AssertCalledByTheRegistry(object value, [CallerMemberName] string test = "")
    {
        Assert.IsNotNull(value);
        Assert.Contains(test, Class.Methods.Select(static method => method.Name));
        Assert.Contains(Registry, Environment.StackTrace);
        Assert.DoesNotContain(Reflection, Environment.StackTrace);
    }
}
