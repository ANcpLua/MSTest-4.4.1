namespace MSTest.Platform.Changed.AzureDevOps;

/// <summary>A test that passes, for the log of the runs that <see cref="LogGroupRunTests"/> reads.</summary>
[TestClass]
public sealed class PackingTests
{
    private static readonly string[] Parcel = ["apple", "pear"];

    [TestMethod]
    public void Packs_both_items() => Assert.HasCount(2, Parcel);
}
