namespace MSTest.Data.DisplayNames;

[TestClass]
public sealed class ArgumentLabelTests(TestContext testContext)
{
    [TestMethod(DisplayName = "invoice")]
    [DataRow(12, ArgumentsDisplayName = "dozen")]
    public void Inline_label_keeps_the_test_display_name(int quantity)
    {
        Assert.AreEqual(12, quantity);
        Assert.AreEqual("invoice (dozen)", testContext.TestDisplayName);
    }

    [TestMethod(DisplayName = "shipment")]
    [DynamicData(nameof(Rows))]
    public void Dynamic_label_keeps_the_test_display_name(int quantity)
    {
        Assert.AreEqual(24, quantity);
        Assert.AreEqual("shipment (two dozen)", testContext.TestDisplayName);
    }

    public static IEnumerable<TestDataRow<int>> Rows =>
        [new(24) { ArgumentsDisplayName = "two dozen" }];

}
