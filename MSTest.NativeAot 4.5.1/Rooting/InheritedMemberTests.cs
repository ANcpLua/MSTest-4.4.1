using MSTest.SourceGeneration.Library.Lifecycle;

namespace MSTest.NativeAot.Rooting;

/// <summary>
/// A test class whose base class comes from another assembly. The generator keeps the members of the base
/// class in the trimmed executable, so MSTest sets the inherited <see cref="TestContext"/> property and
/// runs the inherited class initializer and test initializer there as it does under <c>dotnet run</c>.
/// </summary>
[TestClass]
public sealed class InheritedMemberTests : LifecycleTests
{
    [TestMethod]
    public void Inherited_test_context_is_set()
        => Assert.AreEqual(nameof(Inherited_test_context_is_set), TestContext.TestName);

    [TestMethod]
    public void Inherited_test_initializer_runs()
        => Assert.AreEqual(nameof(Inherited_test_initializer_runs), InitializedTest);

    [TestMethod]
    public void Inherited_class_initializer_runs()
        => Assert.Contains(typeof(InheritedMemberTests).FullName!, InitializedClasses);
}
