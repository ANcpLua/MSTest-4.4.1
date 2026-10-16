namespace MSTest.Platform.Terminal;

/// <summary>
/// Three stock counts of different length: the whole warehouse takes longest, one aisle half as long and one
/// shelf is counted at once. <see cref="SlowestTestsRunTests"/> runs them to rank them by their duration.
/// </summary>
[TestClass]
public sealed class StockCountTests(TestContext testContext)
{
    private static readonly TimeSpan Warehouse = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan Aisle = TimeSpan.FromMilliseconds(200);

    [TestMethod]
    public void Counts_one_shelf() => Assert.AreEqual(12, Count(boxes: 3, itemsInABox: 4));

    [TestMethod]
    public async Task Counts_the_whole_warehouse()
    {
        await Task.Delay(Warehouse, testContext.CancellationToken);

        Assert.AreEqual(4800, Count(boxes: 1200, itemsInABox: 4));
    }

    [TestMethod]
    public async Task Counts_one_aisle()
    {
        await Task.Delay(Aisle, testContext.CancellationToken);

        Assert.AreEqual(240, Count(boxes: 60, itemsInABox: 4));
    }

    private static int Count(int boxes, int itemsInABox) => boxes * itemsInABox;
}
