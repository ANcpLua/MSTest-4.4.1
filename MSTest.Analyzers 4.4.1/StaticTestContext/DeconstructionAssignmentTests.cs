namespace MSTest.Analyzers.StaticTestContext;

/// <summary>
/// MSTEST0024 reports a deconstruction that stores the <see cref="TestContext"/> parameter of a fixture in a
/// static member, next to the other value that it assigns. The stored context is the one of the assembly
/// fixture and knows no test class: read in a test, its <see cref="TestContext.FullyQualifiedTestClassName"/>
/// throws an <see cref="InvalidOperationException"/>. The accepted deconstruction stores values that the
/// fixture reads from its context, and a test takes its own context through the constructor.
/// </summary>
[TestClass]
public sealed class DeconstructionAssignmentTests(TestContext testContext)
{
    private static TestContext? fixtureContext;

    private static string? deploymentDirectory;

    private static string? runDirectory;

    private static string? resultsDirectory;

    [AssemblyInitialize]
    public static void BeforeAllTests(TestContext context)
    {
#pragma warning disable MSTEST0024
        (fixtureContext, deploymentDirectory) = (context, context.DeploymentDirectory);
#pragma warning restore MSTEST0024

        (runDirectory, resultsDirectory) = (context.TestRunDirectory, context.ResultsDirectory);
    }

    [TestMethod]
    public void Stored_context_does_not_know_the_class()
        => Assert.ThrowsExactly<InvalidOperationException>(() => fixtureContext!.FullyQualifiedTestClassName);

    [TestMethod]
    public void Stored_values_are_read_in_the_test()
    {
        Assert.IsNotNull(deploymentDirectory);
        Assert.AreEqual(testContext.DeploymentDirectory, deploymentDirectory);
        Assert.IsNotNull(runDirectory);
        Assert.AreEqual(testContext.TestRunDirectory, runDirectory);
        Assert.IsNotNull(resultsDirectory);
        Assert.AreEqual(testContext.ResultsDirectory, resultsDirectory);
    }

    [TestMethod]
    public void Own_context_knows_the_class()
        => Assert.AreEqual(typeof(DeconstructionAssignmentTests).FullName, testContext.FullyQualifiedTestClassName);
}
