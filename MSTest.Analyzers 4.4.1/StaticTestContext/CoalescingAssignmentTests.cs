namespace MSTest.Analyzers.StaticTestContext;

/// <summary>
/// MSTEST0024 reports a coalescing assignment that stores the <see cref="TestContext"/> parameter of a fixture
/// in a static member. The stored context is the one of the class fixture and knows no test: read in a test,
/// its <see cref="TestContext.TestName"/> throws an <see cref="InvalidOperationException"/>. The accepted
/// assignment stores the value that the fixture reads from its context, and a test takes its own context
/// through the constructor.
/// </summary>
[TestClass]
public sealed class CoalescingAssignmentTests(TestContext testContext)
{
    private static TestContext? fixtureContext;

    private static string? className;

    [ClassInitialize]
    public static void BeforeTheTestsOfThisClass(TestContext context)
    {
#pragma warning disable MSTEST0024
        fixtureContext ??= context;
#pragma warning restore MSTEST0024

        className ??= context.FullyQualifiedTestClassName;
    }

    [TestMethod]
    public void Stored_context_does_not_know_the_test()
        => Assert.ThrowsExactly<InvalidOperationException>(() => fixtureContext!.TestName);

    [TestMethod]
    public void Stored_value_is_read_in_the_test()
        => Assert.AreEqual(typeof(CoalescingAssignmentTests).FullName, className);

    [TestMethod]
    public void Own_context_knows_the_test()
        => Assert.AreEqual(nameof(Own_context_knows_the_test), testContext.TestName);
}
