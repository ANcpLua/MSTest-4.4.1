using System.Runtime.CompilerServices;
using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.Descriptors;

/// <summary>
/// The generator writes a descriptor for a test that is a public, synchronous instance method without a
/// return value and carries <c>[TestMethod]</c> and <c>[DataRow]</c> only. Every test of this class is one,
/// the class is not partial and <c>[TestClass]</c> is its only attribute, so the generator marks the
/// descriptors of the class as complete. MSTest on Microsoft.Testing.Platform then takes the tests of the
/// class from the descriptors and neither looks its methods up nor validates them at run time.
/// </summary>
[TestClass]
public sealed class SupportedShapeTests
{
    private static TestClassReflectionInfo Class
        => MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(SupportedShapeTests));

    [TestMethod]
    public void Test_without_data_has_a_descriptor() => Assert.IsTrue(HasDescriptor());

    [TestMethod]
    [DataRow(1, 2, 3)]
    [DataRow(2, 2, 4)]
    public void Test_with_data_rows_has_a_descriptor(int left, int right, int sum)
    {
        Assert.AreEqual(sum, left + right);
        Assert.IsTrue(HasDescriptor());
    }

    [TestMethod]
    public void Descriptors_of_the_class_are_complete()
    {
        Assert.IsTrue(Class.SupportsGeneratedDescriptors);
        Assert.IsTrue(Class.AreGeneratedDescriptorsComplete);
    }

    private static bool HasDescriptor([CallerMemberName] string test = "")
        => Class.Methods.Single(method => method.Name == test).IsDescriptorSupported;
}
