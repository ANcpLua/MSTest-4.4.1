namespace MSTest.SourceGeneration.Library.Fixtures;

/// <summary>
/// Assembly fixtures that a library shares with test assemblies. A test assembly names this class in
/// <c>[assembly: AssemblyFixtureProvider(typeof(SharedFixtures))]</c>, and MSTest then runs
/// <see cref="Initialize"/> once before the tests of that assembly, unless the assembly belongs to a
/// Native AOT project or declares an <c>[AssemblyInitialize]</c> method of its own.
/// </summary>
[TestClass]
public static class SharedFixtures
{
    public static bool Initialized { get; private set; }

    [AssemblyInitialize]
    public static void Initialize(TestContext testContext) => Initialized = true;
}
