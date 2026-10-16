using MSTest.Showcase;

using MSTest.Ordering.Setup;

namespace MSTest.Ordering.Analyzers;

/// <summary>
/// MSTEST0078 reports a [DependsOn] that names a type under which no test of the assembly runs: a class
/// that is no test class, an abstract test class and a test class of another assembly. The run ignores
/// such a reference with a warning, so the test does not wait.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class TypeReferenceTests
{
    [TestMethod]
    [DependsOn(typeof(MigrationTests))]
    public void Test_class_is_waited_for() => Assert.IsTrue(MigrationBase.Applied);

    [TestMethod]
#pragma warning disable MSTEST0078
    [DependsOn(typeof(MigrationBase))]
#pragma warning restore MSTEST0078
    public void Class_that_is_no_test_class_orders_nothing() => Assert.IsFalse(MigrationBase.Applied);

    [TestMethod]
#pragma warning disable MSTEST0078
    [DependsOn(typeof(MigrationContract))]
#pragma warning restore MSTEST0078
    public void Abstract_test_class_orders_nothing() => Assert.IsFalse(MigrationBase.Applied);

    [TestMethod]
#pragma warning disable MSTEST0078
    [DependsOn(typeof(DatabaseTests))]
#pragma warning restore MSTEST0078
    public void Test_class_of_another_assembly_orders_nothing() => Assert.IsFalse(MigrationBase.Applied);
}
