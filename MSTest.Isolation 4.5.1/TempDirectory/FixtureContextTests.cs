namespace MSTest.Isolation.TempDirectory;

/// <summary>
/// TestTempDirectory belongs to a test. The TestContext that ClassInitialize receives belongs to no test, and
/// its TestTempDirectory is null.
/// </summary>
[TestClass]
public sealed class FixtureContextTests(TestContext testContext)
{
    [ClassInitialize]
    public static void Finds_no_directory_in_the_class_fixture(TestContext context)
    {
        Assert.IsNull(context.TestTempDirectory);
    }

    [TestMethod]
    public void Finds_a_directory_in_the_test()
    {
        Assert.IsTrue(Directory.Exists(testContext.TestTempDirectory));
    }
}
