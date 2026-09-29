using Microsoft.VisualStudio.TestTools.UnitTesting.Combinatorial;

namespace MSTest.Data.Combinatorial;

/// <summary>
/// <see cref="CombinatorialValuesAttribute"/> lists the values of a parameter, which run in the order they are
/// written. <see langword="null"/> is a value like any other, also when it is the only argument.
/// </summary>
[TestClass]
public sealed class CombinatorialValuesTests
{
    [TestMethod]
    [CombinatorialData]
    public void Runs_for_the_listed_values([CombinatorialValues("DE", "AT", "CH")] string country)
        => Assert.IsTrue(country is "DE" or "AT" or "CH");

    [TestMethod]
    [CombinatorialData]
    public void Null_is_a_value_like_any_other([CombinatorialValues("gift", null)] string? note)
        => Assert.IsTrue(note is "gift" or null);

    [TestMethod]
    [CombinatorialData]
    public void A_single_null_is_one_value([CombinatorialValues(null)] string? note)
        => Assert.IsNull(note);

    [TestMethod]
    [DataRow(nameof(Runs_for_the_listed_values), new object[] { "DE", "AT", "CH" })]
    [DataRow(nameof(Null_is_a_value_like_any_other), new object?[] { "gift", null })]
    [DataRow(nameof(A_single_null_is_one_value), new object?[] { null })]
    public void The_values_come_in_the_order_they_are_written(string test, object?[] expected)
        => Assert.AreSequenceEqual(
            expected,
            new CombinatorialDataAttribute()
                .GetData(typeof(CombinatorialValuesTests).GetMethod(test)!)
                .Select(row => row[0]));
}
