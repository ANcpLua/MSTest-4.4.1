namespace MSTest.Ordering.Analyzers;

/// <summary>
/// MSTEST0078 reports a [DependsOn] that no test reads: on a method that is no test, and on a class that
/// runs no test of its own, such as the base class of a test class. The test that calls the method and the
/// test that the base class declares do not wait for Migrate.
/// </summary>
[TestClass]
public sealed class PlacementTests : PlacementBase
{
    [TestMethod]
    [DependsOn(typeof(MigrationTests))]
    public void Attribute_on_a_test_is_waited_for() => Assert.IsTrue(MigrationBase.Applied);

    [TestMethod]
    public void Attribute_on_a_method_that_is_no_test_orders_nothing() => Check();

#pragma warning disable MSTEST0078
    [DependsOn(typeof(MigrationTests))]
#pragma warning restore MSTEST0078
    private static void Check() => Assert.IsFalse(MigrationBase.Applied);
}

/// <summary>The base class whose [DependsOn] does not reach the tests of <see cref="PlacementTests"/>.</summary>
#pragma warning disable MSTEST0078
[DependsOn(typeof(MigrationTests))]
#pragma warning restore MSTEST0078
public abstract class PlacementBase
{
    [TestMethod]
    public void Attribute_on_the_base_class_orders_nothing() => Assert.IsFalse(MigrationBase.Applied);
}

/// <summary>The accepted placement on a class: the test class itself carries the [DependsOn].</summary>
[TestClass]
[DependsOn(typeof(MigrationTests))]
public sealed class AcceptedPlacementTests
{
    [TestMethod]
    public void Attribute_on_the_test_class_is_waited_for() => Assert.IsTrue(MigrationBase.Applied);
}
