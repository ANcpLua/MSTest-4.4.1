namespace MSTest.Isolation.TempDirectory;

/// <summary>
/// The TestTempDirectory of a test that passed is deleted together with its files. The second test runs after
/// the first one and no longer finds the directory.
/// </summary>
[TestClass]
public sealed class PassedTestCleanupTests(TestContext testContext)
{
    private static volatile string? directory;

    [TestMethod]
    public void Writes_a_file_and_passes()
    {
        directory = testContext.TestTempDirectory!;

        File.WriteAllText(Path.Combine(directory, "scratch.txt"), "no longer needed");

        Assert.IsTrue(Directory.Exists(directory));
    }

    [TestMethod]
    [DependsOn(nameof(Writes_a_file_and_passes))]
    public void Finds_the_directory_deleted()
    {
        Assert.IsNotNull(directory);
        Assert.IsFalse(Directory.Exists(directory));
    }
}
