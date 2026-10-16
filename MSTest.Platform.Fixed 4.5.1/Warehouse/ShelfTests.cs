namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>A test that passes, for the runs of the examples that need a green test.</summary>
[TestClass]
public sealed class ShelfTests
{
    private static readonly string[] Boxes = ["apples", "pears", "plums"];

    [TestMethod]
    public void Counts_the_boxes_on_the_shelf() => Assert.HasCount(3, Boxes);
}
