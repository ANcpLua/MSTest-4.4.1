using MSTest.Showcase;

namespace MSTest.Isolation.TempDirectory;

/// <summary>
///     With the environment variable MSTEST_TEST_TEMP_DIRECTORY_RETAIN set to 1 or true, the TestTempDirectory of a
///     passing test is kept as well. The second test runs after the first one, finds the file and removes the
///     directory.
///     Expected outcome with MSTEST_EXPLICIT=1, MSTEST_TEST_TEMP_DIRECTORY_RETAIN=1 and a filter on this class:
///     both tests pass. Without MSTEST_TEST_TEMP_DIRECTORY_RETAIN, Finds_the_file_kept fails.
/// </summary>
[TestClass]
[Explicit]
public sealed class RetainedByEnvironmentShowcase(TestContext testContext)
{
    private static volatile string? _file;

    [TestMethod]
    public void Writes_a_file_and_passes()
    {
        _file = Path.Combine(testContext.TestTempDirectory!, "state.txt");

        File.WriteAllText(_file, "kept for debugging");
    }

    [TestMethod]
    [DependsOn(nameof(Writes_a_file_and_passes))]
    public void Finds_the_file_kept()
    {
        Assert.IsNotNull(_file);
        Assert.AreEqual("kept for debugging", File.ReadAllText(_file));

        Directory.Delete(Path.GetDirectoryName(_file)!, true);
    }
}