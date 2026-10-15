using System.Collections.Concurrent;

namespace MSTest.Isolation.TempDirectory;

/// <summary>
///     Every row of a data-driven test has a TestTempDirectory of its own: each row finds its directory empty,
///     and no two rows receive the same path.
/// </summary>
[TestClass]
public sealed class DataRowDirectoryTests(TestContext testContext)
{
    private static readonly ConcurrentDictionary<string, string> Directories = new();

    [TestMethod]
    [DataRow("alpha")]
    [DataRow("beta")]
    [DataRow("gamma")]
    public void Writes_the_row_into_a_directory_of_its_own(string row)
    {
        var directory = testContext.TestTempDirectory!;

        Assert.IsTrue(Directories.TryAdd(directory, row));
        Assert.IsEmpty(Directory.GetFileSystemEntries(directory));

        File.WriteAllText(Path.Combine(directory, "row.txt"), row);
    }
}