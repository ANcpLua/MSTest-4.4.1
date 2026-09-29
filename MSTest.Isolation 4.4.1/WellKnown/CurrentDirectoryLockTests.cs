namespace MSTest.Isolation.WellKnown;

/// <summary>
/// WellKnownResources.CurrentDirectory is the key for the current directory of the process. Each row enters
/// its own directory and resolves a relative path there because the other row waits for the lock.
/// </summary>
[TestClass]
public sealed class CurrentDirectoryLockTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    [TestMethod]
    [ResourceLock(WellKnownResources.CurrentDirectory)]
    [DataRow("first.txt")]
    [DataRow("second.txt")]
    public async Task Resolves_a_relative_path_in_the_directory_it_entered(string file)
    {
        string entered = testContext.TestTempDirectory!;
        string previous = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(entered);

        try
        {
            await Task.Delay(Hold, testContext.CancellationToken);

            Assert.AreEqual(Path.Combine(entered, file), Path.GetFullPath(file));
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
        }
    }
}
