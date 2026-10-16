using MSTest.Showcase;

namespace MSTest.Ordering.Dependencies;

/// <summary>
///     A [DoNotParallelize] test runs after the parallel tests of the assembly. A test that depends on it is
///     moved behind it, although it is parallelizable itself.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class SequentialPrerequisiteTests
{
    private static volatile bool _migrated;

    [TestMethod]
    [DependsOn(nameof(MigrateDatabase))]
    public void Runs_after_the_sequential_test()
    {
        Assert.IsTrue(_migrated);
    }

    [TestMethod]
    [DoNotParallelize]
    public void MigrateDatabase()
    {
        _migrated = true;
    }
}