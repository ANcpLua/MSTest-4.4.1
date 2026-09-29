namespace MSTest.NativeAot.AssemblyFixtures;

/// <summary>
/// The assembly fixture of a Native AOT project is declared in a test class of the project itself. MSTest
/// looks for <c>[AssemblyInitialize]</c> in the classes that the generated registry lists and runs it
/// before the tests. A fixture that <c>[AssemblyFixtureProvider]</c> names is not found in such a project.
/// </summary>
[TestClass]
public sealed class AssemblyFixtureTests
{
    private static bool _initialized;

    [AssemblyInitialize]
    public static void Initialize(TestContext testContext) => _initialized = true;

    [TestMethod]
    public void Assembly_initializer_of_the_project_runs() => Assert.IsTrue(_initialized);
}
