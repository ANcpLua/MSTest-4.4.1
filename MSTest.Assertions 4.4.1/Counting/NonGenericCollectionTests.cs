namespace MSTest.Assertions.Counting;

/// <summary>
///     <c>Assert.HasCount</c> and <c>Assert.IsEmpty</c> read <c>ICollection.Count</c> of a non-generic collection
///     and leave it unenumerated. A non-generic sequence without a count is enumerated once.
/// </summary>
[TestClass]
public sealed class NonGenericCollectionTests
{
    [TestMethod]
    public void HasCount_reads_the_count_of_a_collection()
    {
        Shelf shelf = new(3);

        Assert.HasCount(3, shelf);

        Assert.AreEqual(0, shelf.Enumerations);
    }

    [TestMethod]
    public void HasCount_fails_on_another_count_without_enumerating()
    {
        Shelf shelf = new(3);

        var failure = Assert.ThrowsExactly<AssertFailedException>(() => Assert.HasCount(4, shelf));

        Assert.Contains("actual count:   3", failure.Message);
        Assert.AreEqual(0, shelf.Enumerations);
    }

    [TestMethod]
    public void IsEmpty_reads_the_count_of_a_collection()
    {
        Shelf shelf = new(0);

        Assert.IsEmpty(shelf);

        Assert.AreEqual(0, shelf.Enumerations);
    }

    [TestMethod]
    public void IsEmpty_fails_on_elements_without_enumerating()
    {
        Shelf shelf = new(3);

        var failure = Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsEmpty(shelf));

        Assert.Contains("actual count:   3", failure.Message);
        Assert.AreEqual(0, shelf.Enumerations);
    }

    [TestMethod]
    public void A_sequence_without_a_count_is_enumerated_once()
    {
        Conveyor counted = new(3);
        Conveyor empty = new(0);

        Assert.HasCount(3, counted);
        Assert.IsEmpty(empty);

        Assert.AreEqual(1, counted.Enumerations);
        Assert.AreEqual(1, empty.Enumerations);
    }
}