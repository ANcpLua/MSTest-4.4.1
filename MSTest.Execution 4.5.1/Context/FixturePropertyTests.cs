namespace MSTest.Execution.Context;

/// <summary>
///     MSTEST0048 reports a fixture that reads, through a constant key of <see cref="TestContext.Properties" />, an
///     entry that its context does not have: <c>FullyQualifiedTestClassName</c> in an assembly fixture and
///     <c>TestName</c> in a class fixture. Both reported lookups throw a <see cref="KeyNotFoundException" />. The class
///     fixture reads the name of its class, and a test reads both names.
/// </summary>
[TestClass]
public sealed class FixturePropertyTests(TestContext testContext)
{
    private static object? _classNameInTheClassFixture;

    [AssemblyInitialize]
    public static void BeforeAllTests(TestContext assemblyContext)
    {
#pragma warning disable MSTEST0048
        Assert.ThrowsExactly<KeyNotFoundException>(() => assemblyContext.Properties["FullyQualifiedTestClassName"]);
#pragma warning restore MSTEST0048
    }

    [ClassInitialize]
    public static void BeforeTheTestsOfThisClass(TestContext classContext)
    {
        _classNameInTheClassFixture = classContext.Properties["FullyQualifiedTestClassName"];

#pragma warning disable MSTEST0048
        Assert.ThrowsExactly<KeyNotFoundException>(() => classContext.Properties["TestName"]);
#pragma warning restore MSTEST0048
    }

    [TestMethod]
    public void The_class_fixture_has_read_the_name_of_its_class()
    {
        Assert.AreEqual(typeof(FixturePropertyTests).FullName, _classNameInTheClassFixture);
    }

    [TestMethod]
    public void A_test_reads_its_name_and_the_name_of_its_class()
    {
        Assert.AreEqual(nameof(A_test_reads_its_name_and_the_name_of_its_class), testContext.Properties["TestName"]);
        Assert.AreEqual(typeof(FixturePropertyTests).FullName, testContext.Properties["FullyQualifiedTestClassName"]);
    }
}