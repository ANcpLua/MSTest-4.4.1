using MSTest.Showcase;

namespace MSTest.Isolation.TempDirectory;

/// <summary>
///     The TestTempDirectory of a test that failed is kept, so that the files of the test can be inspected. The
///     second test runs after the failure, finds the file and removes the directory.
///     Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Writes_a_file_and_fails fails,
///     Finds_the_file_kept passes.
/// </summary>
[TestClass]
[Explicit]
public sealed class FailedTestRetentionShowcase(TestContext testContext)
{
    private static volatile string? _file;

    [TestMethod]
    public void Writes_a_file_and_fails()
    {
        _file = Path.Combine(testContext.TestTempDirectory!, "state.txt");

        File.WriteAllText(_file, "state at the time of the failure");

        Assert.Fail("deliberate failure");
    }

    [TestMethod]
    [DependsOn(nameof(Writes_a_file_and_fails), ProceedOnFailure = true)]
    public void Finds_the_file_kept()
    {
        Assert.IsNotNull(_file);
        Assert.AreEqual("state at the time of the failure", File.ReadAllText(_file));

        Directory.Delete(Path.GetDirectoryName(_file)!, true);
    }
}