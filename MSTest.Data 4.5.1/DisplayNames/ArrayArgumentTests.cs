namespace MSTest.Data.DisplayNames;

/// <summary>
/// An array argument is named by its elements in square brackets, and an array inside an array by brackets
/// inside brackets. A test whose only parameter is an <c>object[]</c> takes the whole row as that array.
/// </summary>
[TestClass]
public sealed class ArrayArgumentTests(TestContext testContext)
{
    public static IEnumerable<(int[][] Shelves, string Hall)> Halls => [([[1, 2], [], [3]], "north")];

    [TestMethod]
    [DataRow(new[] { 1, 2, 3 }, new[] { "pen", "ink" })]
    public void Puts_an_array_in_brackets(int[] quantities, string[] items)
        => Assert.AreEqual(
            $"""{nameof(Puts_an_array_in_brackets)} ([1,2,3],["pen","ink"])""",
            testContext.TestDisplayName);

    [TestMethod]
    [DynamicData(nameof(Halls))]
    public void Puts_a_nested_array_in_nested_brackets(int[][] shelves, string hall)
        => Assert.AreEqual(
            $"""{nameof(Puts_a_nested_array_in_nested_brackets)} ([[1,2],[],[3]],"north")""",
            testContext.TestDisplayName);

    [TestMethod]
    [DataRow(new object?[] { "pen", new[] { 'A', 'B' }, null }, 2)]
    public void Names_each_element_of_a_mixed_array_by_its_kind(object?[] order, int parcels)
        => Assert.AreEqual(
            $"""{nameof(Names_each_element_of_a_mixed_array_by_its_kind)} (["pen",['A','B'],null],2)""",
            testContext.TestDisplayName);

    [TestMethod]
    [DataRow("pen", 3, null)]
    public void Takes_the_whole_row_as_the_array_of_its_only_parameter(object?[] order)
        => Assert.AreEqual(
            $"""{nameof(Takes_the_whole_row_as_the_array_of_its_only_parameter)} (["pen",3,null])""",
            testContext.TestDisplayName);
}
