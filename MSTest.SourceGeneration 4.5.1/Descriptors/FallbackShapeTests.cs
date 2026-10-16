using System.Runtime.CompilerServices;
using MSTest.SourceGenerated;

namespace MSTest.SourceGeneration.Descriptors;

/// <summary>
/// A test gets no descriptor when it is asynchronous, returns a task, takes its data from
/// <c>[DynamicData]</c> or carries another attribute than <c>[TestMethod]</c> and <c>[DataRow]</c>. MSTest
/// finds such a test by looking the methods of the class up at run time, and the other tests of the same
/// class keep their descriptors. The descriptors of the class are then incomplete.
/// </summary>
[TestClass]
public sealed class FallbackShapeTests
{
    private static TestClassReflectionInfo Class
        => MSTestReflectionMetadata.TestClasses.Single(static entry => entry.Type == typeof(FallbackShapeTests));

    private static IEnumerable<object[]> Sums => [[1, 2, 3]];

    [TestMethod]
    public async Task Asynchronous_test_has_no_descriptor()
    {
        await Task.Yield();

        Assert.IsFalse(HasDescriptor());
    }

    [TestMethod]
    public Task Test_that_returns_a_task_has_no_descriptor()
    {
        Assert.IsFalse(HasDescriptor());

        return Task.CompletedTask;
    }

    [TestMethod]
    [DynamicData(nameof(Sums))]
    public void Test_with_dynamic_data_has_no_descriptor(int left, int right, int sum)
    {
        Assert.AreEqual(sum, left + right);
        Assert.IsFalse(HasDescriptor());
    }

    [TestMethod]
    [TestCategory("Fallback")]
    public void Test_with_another_attribute_has_no_descriptor() => Assert.IsFalse(HasDescriptor());

    [TestMethod]
    public void Supported_test_of_the_same_class_keeps_its_descriptor() => Assert.IsTrue(HasDescriptor());

    [TestMethod]
    [DataRow(2, 2, 4)]
    public void Supported_test_with_a_data_row_keeps_its_descriptor(int left, int right, int sum)
    {
        Assert.AreEqual(sum, left + right);
        Assert.IsTrue(HasDescriptor());
    }

    [TestMethod]
    public void Descriptors_of_the_class_are_incomplete()
    {
        Assert.IsTrue(Class.SupportsGeneratedDescriptors);
        Assert.IsFalse(Class.AreGeneratedDescriptorsComplete);
    }

    private static bool HasDescriptor([CallerMemberName] string test = "")
        => Class.Methods.Single(method => method.Name == test).IsDescriptorSupported;
}
