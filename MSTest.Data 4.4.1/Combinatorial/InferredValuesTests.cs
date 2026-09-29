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
    public void A_bool_is_true_and_false(bool express)
        => Assert.AreEqual(express, testContext.TestData![0]);

    [TestMethod]
    [CombinatorialData]
    public void An_int_is_zero_and_one(int retries)
        => Assert.IsInRange(0, 1, retries);

    [TestMethod]
    [CombinatorialData]
    public void An_enum_is_each_of_its_members(Priority priority)
        => Assert.IsTrue(Enum.IsDefined(priority));

    [TestMethod]
    [CombinatorialData]
    public void A_nullable_type_starts_with_null(Priority? priority)
        => Assert.IsTrue(priority is null || Enum.IsDefined(priority.Value));

    [TestMethod]
    [DataRow(nameof(A_bool_is_true_and_false), new object[] { true, false })]
    [DataRow(nameof(An_int_is_zero_and_one), new object[] { 0, 1 })]
    [DataRow(nameof(An_enum_is_each_of_its_members), new object[] { Priority.Low, Priority.Normal, Priority.High })]
    [DataRow(nameof(A_nullable_type_starts_with_null), new object?[] { null, Priority.Low, Priority.Normal, Priority.High })]
    public void The_values_come_in_this_order(string test, object?[] expected)
        => Assert.AreSequenceEqual(
            expected,
            new CombinatorialDataAttribute()
                .GetData(typeof(InferredValuesTests).GetMethod(test)!)
                .Select(row => row[0]));
}
