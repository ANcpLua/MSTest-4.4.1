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
    public void A_seed_repeats_the_values(
        [CombinatorialRandomData(Count = 3, Minimum = 10, Maximum = 20, Seed = 42)]
        int discount)
    {
        Assert.IsInRange(10, 20, discount);
    }

    [TestMethod]
    [CombinatorialData]
    public void Without_a_seed_the_values_change([CombinatorialRandomData] int orderNumber)
    {
        Assert.IsInRange(0, int.MaxValue - 1, orderNumber);
    }

    [TestMethod]
    [CombinatorialData]
    public void A_seed_of_zero_is_no_seed([CombinatorialRandomData(Seed = 0)] int orderNumber)
    {
        Assert.IsInRange(0, int.MaxValue - 1, orderNumber);
    }

    [TestMethod]
    [CombinatorialData]
    public void A_count_as_large_as_the_range_takes_every_value(
        [CombinatorialRandomData(Count = 3, Minimum = 1, Maximum = 3)]
        int shelf)
    {
        Assert.IsInRange(1, 3, shelf);
    }

    [TestMethod]
    public void A_seed_generates_the_same_values_again()
    {
        Assert.AreSequenceEqual(
            ValuesOf(nameof(A_seed_repeats_the_values)),
            ValuesOf(nameof(A_seed_repeats_the_values)));
    }

    [TestMethod]
    [DataRow(nameof(Without_a_seed_the_values_change))]
    [DataRow(nameof(A_seed_of_zero_is_no_seed))]
    public void Without_a_seed_the_values_are_drawn_anew(string test)
    {
        Assert.AreNotSequenceEqual(ValuesOf(test), ValuesOf(test));
    }

    [TestMethod]
    public void The_whole_range_comes_in_ascending_order()
    {
        Assert.AreSequenceEqual(
            [1, 2, 3],
            ValuesOf(nameof(A_count_as_large_as_the_range_takes_every_value)));
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
            .GetMethod(nameof(A_seed_repeats_the_values))!
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