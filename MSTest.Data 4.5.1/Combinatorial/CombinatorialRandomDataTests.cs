using Microsoft.VisualStudio.TestTools.UnitTesting.Combinatorial;

namespace MSTest.Data.Combinatorial;

/// <summary>
///     <see cref="CombinatorialRandomDataAttribute" /> generates <c>Count</c> different <see cref="int" /> values
///     from <c>Minimum</c> to <c>Maximum</c>, both inclusive. A <c>Seed</c> other than
///     <see cref="CombinatorialRandomDataAttribute.NoSeed" />, which is 0, generates the same values every time.
///     Without a seed every run draws its values anew, and the names of the rows follow the values. A
///     <c>Count</c> as large as the range leaves nothing to draw and yields the whole range in ascending order.
/// </summary>
[TestClass]
public sealed class CombinatorialRandomDataTests
{
    [TestMethod]
    [CombinatorialData]
    public void Seeded_rows_stay_in_the_requested_range(
        [CombinatorialRandomData(Count = 3, Minimum = 10, Maximum = 20, Seed = 42)]
        int discount)
    {
        Assert.IsInRange(10, 20, discount);
    }

    [TestMethod]
    [CombinatorialData]
    public void Unseeded_rows_stay_in_the_default_range([CombinatorialRandomData] int orderNumber)
    {
        Assert.IsInRange(0, int.MaxValue - 1, orderNumber);
    }

    [TestMethod]
    [CombinatorialData]
    public void Explicitly_unseeded_rows_stay_in_the_default_range([CombinatorialRandomData(Seed = 0)] int orderNumber)
    {
        Assert.IsInRange(0, int.MaxValue - 1, orderNumber);
    }

    [TestMethod]
    [CombinatorialData]
    public void Rows_from_the_whole_range_stay_in_range(
        [CombinatorialRandomData(Count = 3, Minimum = 1, Maximum = 3)]
        int shelf)
    {
        Assert.IsInRange(1, 3, shelf);
    }

    [TestMethod]
    public void A_seed_generates_the_same_values_again()
    {
        Assert.AreSequenceEqual(
            ValuesOf(nameof(Seeded_rows_stay_in_the_requested_range)),
            ValuesOf(nameof(Seeded_rows_stay_in_the_requested_range)));
    }

    [TestMethod]
    [DataRow(nameof(Unseeded_rows_stay_in_the_default_range))]
    [DataRow(nameof(Explicitly_unseeded_rows_stay_in_the_default_range))]
    public void Unseeded_sources_return_five_distinct_values_in_range(string test)
    {
        var values = ValuesOf(test).ToArray();
        Assert.HasCount(5, values);
        Assert.AreAllDistinct(values);
        foreach (var value in values) Assert.IsInRange(0, int.MaxValue - 1, value);
    }

    [TestMethod]
    public void Reenumerating_one_attribute_reuses_its_sample()
    {
        var parameter = typeof(CombinatorialRandomDataTests)
            .GetMethod(nameof(Unseeded_rows_stay_in_the_default_range))!.GetParameters()[0];
        CombinatorialRandomDataAttribute source = new();
        var first = source.GetValues(parameter).ToArray();
        var second = source.GetValues(parameter).ToArray();

        Assert.HasCount(5, first);
        Assert.AreAllDistinct(first);
        Assert.AreSequenceEqual(first, second);
    }

    [TestMethod]
    public void The_whole_range_comes_in_ascending_order()
    {
        Assert.AreSequenceEqual(
            [1, 2, 3],
            ValuesOf(nameof(Rows_from_the_whole_range_stay_in_range)));
    }

    [TestMethod]
    public void The_defaults_are_five_values_up_to_the_largest_int_but_one_without_a_seed()
    {
        CombinatorialRandomDataAttribute defaults = new();

        Assert.AreEqual(5, defaults.Count);
        Assert.AreEqual(0, defaults.Minimum);
        Assert.AreEqual(int.MaxValue - 1, defaults.Maximum);
        Assert.AreEqual(CombinatorialRandomDataAttribute.NoSeed, defaults.Seed);
    }

    [TestMethod]
    public void A_range_with_fewer_values_than_the_count_is_rejected()
    {
        var discount = typeof(CombinatorialRandomDataTests)
            .GetMethod(nameof(Seeded_rows_stay_in_the_requested_range))!
            .GetParameters()[0];

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            new CombinatorialRandomDataAttribute { Count = 0 }.GetValues(discount));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            new CombinatorialRandomDataAttribute { Minimum = 2, Maximum = 1 }.GetValues(discount));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            new CombinatorialRandomDataAttribute { Count = 3, Minimum = 1, Maximum = 2 }.GetValues(discount));
    }

    private static IEnumerable<int> ValuesOf(string test)
    {
        return new CombinatorialDataAttribute()
            .GetData(typeof(CombinatorialRandomDataTests).GetMethod(test)!)
            .Select(row => (int)row[0]!);
    }
}