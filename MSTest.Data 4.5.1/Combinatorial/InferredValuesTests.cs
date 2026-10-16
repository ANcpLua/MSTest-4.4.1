using Microsoft.VisualStudio.TestTools.UnitTesting.Combinatorial;

namespace MSTest.Data.Combinatorial;

/// <summary>
/// A parameter without a provider attribute takes its values from its type: <see langword="true"/> and
/// <see langword="false"/> for <see cref="bool"/>, 0 and 1 for <see cref="int"/> and every member for an
/// enum. The nullable form of these types starts with <see langword="null"/>.
/// </summary>
[TestClass]
public sealed class InferredValuesTests(TestContext testContext)
{
    [TestMethod]
    [CombinatorialData]
    public void Boolean_row_matches_the_current_context_argument(bool express)
        => Assert.AreEqual(express, testContext.TestData![0]);

    [TestMethod]
    [CombinatorialData]
    public void Integer_rows_are_zero_or_one(int retries)
        => Assert.IsInRange(0, 1, retries);

    [TestMethod]
    [CombinatorialData]
    public void Enum_rows_are_declared_members(Priority priority)
        => Assert.IsTrue(Enum.IsDefined(priority));

    [TestMethod]
    [CombinatorialData]
    public void Nullable_enum_rows_are_null_or_declared_members(Priority? priority)
        => Assert.IsTrue(priority is null || Enum.IsDefined(priority.Value));

    [TestMethod]
    [DataRow(nameof(Boolean_row_matches_the_current_context_argument), new object[] { true, false })]
    [DataRow(nameof(Integer_rows_are_zero_or_one), new object[] { 0, 1 })]
    [DataRow(nameof(Enum_rows_are_declared_members), new object[] { Priority.Low, Priority.Normal, Priority.High })]
    [DataRow(nameof(Nullable_enum_rows_are_null_or_declared_members), new object?[] { null, Priority.Low, Priority.Normal, Priority.High })]
    public void The_values_come_in_this_order(string test, object?[] expected)
        => Assert.AreSequenceEqual(
            expected,
            new CombinatorialDataAttribute()
                .GetData(typeof(InferredValuesTests).GetMethod(test)!)
                .Select(row => row[0]));
}
