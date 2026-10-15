namespace MSTest.Analyzers.StaticTestContext;

/// <summary>
///     MSTEST0024 reports a deconstruction that stores the <see cref="TestContext" /> parameter of a fixture in a
///     static member, next to the other value that it assigns. The stored context is the one of the assembly
///     fixture and knows no test class: read in a test, its <see cref="TestContext.FullyQualifiedTestClassName" />
///     throws an <see cref="InvalidOperationException" />. The accepted deconstruction stores values that the
///     fixture reads from its context, and a test takes its own context through the constructor.
/// </summary>
[TestClass]
public sealed class DeconstructionAssignmentTests(TestContext testContext)
{
    private static TestContext? _fixtureContext;

    private static string? _deploymentDirectory;

    private static string? _runDirectory;

    private static string? _resultsDirectory;

    [AssemblyInitialize]
    public static void BeforeAllTests(TestContext context)
    {
#pragma warning disable MSTEST0024
        (_fixtureContext, _deploymentDirectory) = (context, context.DeploymentDirectory);
#pragma warning restore MSTEST0024

        (_runDirectory, _resultsDirectory) = (context.TestRunDirectory, context.ResultsDirectory);
    }

    [TestMethod]
    public void Stored_context_does_not_know_the_class()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _fixtureContext!.FullyQualifiedTestClassName);
    }

    [TestMethod]
    public void Stored_values_are_read_in_the_test()
    {
        Assert.IsNotNull(_deploymentDirectory);
        Assert.AreEqual(testContext.DeploymentDirectory, _deploymentDirectory);
        Assert.IsNotNull(_runDirectory);
        Assert.AreEqual(testContext.TestRunDirectory, _runDirectory);
        Assert.IsNotNull(_resultsDirectory);
        Assert.AreEqual(testContext.ResultsDirectory, _resultsDirectory);
    }

    [TestMethod]
    public void Own_context_knows_the_class()
    {
        Assert.AreEqual(typeof(DeconstructionAssignmentTests).FullName, testContext.FullyQualifiedTestClassName);
    }
}