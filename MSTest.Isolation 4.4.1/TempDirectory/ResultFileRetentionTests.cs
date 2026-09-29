namespace MSTest.Isolation.TempDirectory;

/// <summary>
/// A passing test that registers a file of its TestTempDirectory with TestContext.AddResultFile keeps the
/// directory, so that the host can collect the attachment. The second test runs after the first one, finds
/// the file and removes the directory.
/// </summary>
[TestClass]
public sealed class ResultFileRetentionTests(TestContext testContext)
{
    private static volatile string? file;

    [TestMethod]
    public void Registers_a_file_and_passes()
    {
        file = Path.Combine(testContext.TestTempDirectory!, "attachment.txt");

        File.WriteAllText(file, "attached");

        testContext.AddResultFile(file);
    }

    [TestMethod]
    [DependsOn(nameof(Registers_a_file_and_passes))]
    public void Finds_the_registered_file_kept()
    {
        Assert.IsNotNull(file);
        Assert.AreEqual("attached", File.ReadAllText(file));

        Directory.Delete(Path.GetDirectoryName(file)!, recursive: true);
    }
}
