using MSTest.Showcase;

namespace MSTest.Ordering.Analyzers;

/// <summary>
///     MSTEST0078 reports a [DependsOn] that names a method which does not exist on the class or is no test.
///     The run ignores such a reference with a warning, so the test does not wait for Migrate.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class MethodReferenceTests
{
    [TestMethod]
    [DependsOn(typeof(MigrationTests), nameof(MigrationTests.Migrate))]
    public void Name_of_a_test_is_waited_for()
    {
        Assert.IsTrue(MigrationBase.Applied);
    }

    [TestMethod]
#pragma warning disable MSTEST0078
    [DependsOn(typeof(MigrationTests), "Migrat")]
#pragma warning restore MSTEST0078
    public void Misspelled_name_orders_nothing() => Assert.IsFalse(MigrationBase.Applied);

    [TestMethod]
#pragma warning disable MSTEST0078
    [DependsOn(typeof(MigrationTests), nameof(MigrationBase.Apply))]
#pragma warning restore MSTEST0078
    public void Name_of_a_method_that_is_no_test_orders_nothing() => Assert.IsFalse(MigrationBase.Applied);
}
