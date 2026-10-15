namespace MSTest.Ordering.Analyzers;

/// <summary>
///     The prerequisite of the MSTEST0078 examples. Migrate is declared in an abstract test class and runs as a
///     test of MigrationTests. [DoNotParallelize] places it behind the parallel tests of the assembly, so a
///     test finds the migration applied only when a dependency holds it back.
/// </summary>
[TestClass]
public sealed class MigrationTests : MigrationContract;

/// <summary>An abstract test class: its test runs under <see cref="MigrationTests" />.</summary>
public abstract class MigrationContract : MigrationBase
{
    [TestMethod]
    [DoNotParallelize]
    public void Migrate()
    {
        Apply();
    }
}

/// <summary>A base class that is no test class, with a method that is no test.</summary>
public abstract class MigrationBase
{
    private static volatile bool _applied;

    internal static bool Applied => _applied;

    internal static void Apply()
    {
        _applied = true;
    }
}