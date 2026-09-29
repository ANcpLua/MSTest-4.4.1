namespace MSTest.Platform.Changed.ServerMode;

/// <summary>
/// A test that ends at once and a test that runs for a second, for the updates that a server sends to its
/// client while <see cref="UpdateBatchRunTests"/> runs them.
/// </summary>
[TestClass]
public sealed class PickingTests(TestContext testContext)
{
    private static readonly string[] Shelf = ["apple", "pear"];

    [TestMethod]
    public void Picks_the_item_at_once() => Assert.Contains("apple", Shelf);

    [TestMethod]
    public async Task Waits_for_the_forklift()
    {
        await Task.Delay(TimeSpan.FromSeconds(1), testContext.CancellationToken);

        Assert.Contains("pear", Shelf);
    }
}
