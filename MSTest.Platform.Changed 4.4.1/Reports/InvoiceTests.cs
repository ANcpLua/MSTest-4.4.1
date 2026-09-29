namespace MSTest.Platform.Changed.Reports;

/// <summary>Two tests about an invoice, each of which a run of <see cref="ExistingFileRunTests"/> reports alone.</summary>
[TestClass]
public sealed class InvoiceTests
{
    private static readonly decimal[] Items = [19.99m, 5.01m];

    [TestMethod]
    public void Adds_the_items() => Assert.AreEqual(25.00m, Items.Sum());

    [TestMethod]
    public void Counts_the_items() => Assert.HasCount(2, Items);
}
