namespace MSTest.Isolation.TempDirectory;

/// <summary>
/// TestContext.TestTempDirectory is a directory for the running test alone. The first read of the property
/// creates it in TestContext.TestResultsDirectory, named after the test, and every later read returns the
/// same path.
/// </summary>
[TestClass]
public sealed class TestTempDirectoryTests(TestContext testContext)
{
    [TestMethod]
    public void Creates_the_directory_on_the_first_read()
    {
        string results = testContext.TestResultsDirectory!;
        string[] before = Directory.GetDirectories(results);

        string directory = testContext.TestTempDirectory!;

        Assert.DoesNotContain(directory, before);
        Assert.Contains(directory, Directory.GetDirectories(results));
        Assert.StartsWith($"{testContext.TestName}_", Path.GetFileName(directory));
    }

    [TestMethod]
    public void Returns_the_same_directory_on_every_read()
    {
        string? first = testContext.TestTempDirectory;
        string? second = testContext.TestTempDirectory;

        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void Holds_the_files_of_the_test()
    {
        string report = Path.Combine(testContext.TestTempDirectory!, "report.json");

        File.WriteAllText(report, "{}");

        Assert.AreEqual("{}", File.ReadAllText(report));
    }
}
