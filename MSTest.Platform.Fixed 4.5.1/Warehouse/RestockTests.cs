using MSTest.Showcase;

namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>
/// A test that passes at once and a test that fails in the first test host and passes in the next one, because
/// the first attempt leaves a note in the directory that the variable <c>MSTEST_SHELF</c> names. Expected
/// outcome with MSTEST_EXPLICIT=1, MSTEST_SHELF set to an empty directory, a filter on this class and
/// <c>--retry-failed-tests 1</c>: the first attempt runs both tests and Finds_the_item_after_the_restock fails,
/// the second attempt runs that test alone and it passes, and the run ends with exit code 0.
/// </summary>
[TestClass]
[Explicit]
public sealed class RestockTests
{
    public const string Shelf = "MSTEST_SHELF";

    private static readonly string[] Items = ["apple", "pear"];

    [TestMethod]
    public void Finds_the_item_on_the_shelf() => Assert.Contains("apple", Items);

    [TestMethod]
    public void Finds_the_item_after_the_restock()
    {
        var note = Path.Combine(Environment.GetEnvironmentVariable(Shelf)!, "restocked");
        var restocked = File.Exists(note);
        File.WriteAllText(note, string.Empty);

        Assert.IsTrue(restocked, "The shelf is empty until the restock.");
    }
}
