namespace MSTest.SourceGeneration.Diagnostics;

/// <summary>
/// AOTSG0001 reports a static class that declares <c>[TestClass]</c>. The generator writes no entry for it,
/// so MSTest does not see the class and its <c>[AssemblyInitialize]</c> method never runs. The assembly
/// fixture belongs in a test class that is not static, where the method itself stays static.
/// </summary>
[TestClass]
public sealed class StaticTestClassTests
{
    private static bool _initialized;

    [AssemblyInitialize]
    public static void Initialize(TestContext testContext) => _initialized = true;

    [TestMethod]
    public void Assembly_initializer_of_the_static_class_does_not_run() => Assert.IsFalse(StaticFixtures.Initialized);

    [TestMethod]
    public void Assembly_initializer_of_the_class_that_is_not_static_runs() => Assert.IsTrue(_initialized);
}

// AOTSG0001
[TestClass]
public static class StaticFixtures
{
    public static bool Initialized { get; private set; }

    [AssemblyInitialize]
    public static void Initialize(TestContext testContext) => Initialized = true;
}
