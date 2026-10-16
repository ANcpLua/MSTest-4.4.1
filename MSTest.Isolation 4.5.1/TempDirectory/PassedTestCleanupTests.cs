namespace MSTest.Isolation.TempDirectory;

/// <summary>
///     The TestTempDirectory of a test that passed is deleted together with its files. The second test runs after
///     the first one and no longer finds the directory.
/// </summary>
[TestClass]
public sealed class PassedTestCleanupTests(TestContext testContext)
{
    private static volatile string? _directory;

    [TestMethod]
    public void Writes_a_file_and_passes()
    {
        _directory = testContext.TestTempDirectory!;

        File.WriteAllText(Path.Combine(_directory, "scratch.txt"), "no longer needed");

        Assert.IsTrue(Directory.Exists(_directory));
    }

    [TestMethod]
    [DependsOn(nameof(Writes_a_file_and_passes))]
    public void Finds_the_directory_deleted()
    {
        Assert.IsNotNull(_directory);
        Assert.IsFalse(Directory.Exists(_directory));
    }
}