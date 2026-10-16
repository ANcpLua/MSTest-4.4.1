using Microsoft.VisualStudio.TestTools.UnitTesting.Combinatorial;

namespace MSTest.Data.Combinatorial;

/// <summary>
/// <see cref="CombinatorialRangeAttribute"/> generates <see cref="int"/> or <see cref="uint"/> values. Two
/// arguments are a start and a count. Three arguments are a start, an inclusive end and a step, and the end
/// is a value only when a step lands on it. A negative step counts down, and so does an unsigned range whose
/// end lies below its start. A count below one, a step of zero and a step that leads away from the end throw
/// <see cref="ArgumentOutOfRangeException"/>.
/// </summary>
[TestClass]
public sealed class CombinatorialRangeTests
{
    [TestMethod]
    [CombinatorialData]
    public void Two_arguments_are_a_start_and_a_count([CombinatorialRange(1, 3)] int parcels)
        => Assert.IsInRange(1, 3, parcels);

    [TestMethod]
    [CombinatorialData]
    public void Three_arguments_are_a_start_an_end_and_a_step([CombinatorialRange(2, 6, 2)] int kilograms)
        {
        Assert.IsInRange(2, 6, kilograms);
        Assert.AreEqual(0, kilograms % 2);
    }

    [TestMethod]
    [CombinatorialData]
    public void The_end_is_a_value_only_when_a_step_lands_on_it([CombinatorialRange(0, 7, 2)] int kilograms)
        {
        Assert.IsInRange(0, 6, kilograms);
        Assert.AreEqual(0, kilograms % 2);
    }

    [TestMethod]
    [CombinatorialData]
    public void A_negative_step_counts_down([CombinatorialRange(7, 0, -2)] int days)
        {
        Assert.IsInRange(1, 7, days);
        Assert.AreEqual(1, days % 2);
    }

    [TestMethod]
    [CombinatorialData]
    public void Unsigned_arguments_generate_unsigned_values([CombinatorialRange(0u, 4u)] uint shelf)
        => Assert.IsInRange(0u, 3u, shelf);

    [TestMethod]
    [CombinatorialData]
    public void An_unsigned_range_counts_down_to_a_lower_end([CombinatorialRange(7u, 0u, 2u)] uint shelf)
        {
        Assert.IsInRange(1u, 7u, shelf);
        Assert.AreEqual(1u, shelf % 2u);
    }

    [TestMethod]
    [DataRow(nameof(Two_arguments_are_a_start_and_a_count), new object[] { 1, 2, 3 })]
    [DataRow(nameof(Three_arguments_are_a_start_an_end_and_a_step), new object[] { 2, 4, 6 })]
    [DataRow(nameof(The_end_is_a_value_only_when_a_step_lands_on_it), new object[] { 0, 2, 4, 6 })]
    [DataRow(nameof(A_negative_step_counts_down), new object[] { 7, 5, 3, 1 })]
    [DataRow(nameof(Unsigned_arguments_generate_unsigned_values), new object[] { 0u, 1u, 2u, 3u })]
    [DataRow(nameof(An_unsigned_range_counts_down_to_a_lower_end), new object[] { 7u, 5u, 3u, 1u })]
    public void The_values_come_in_this_order(string test, object[] expected)
        => Assert.AreSequenceEqual(
            expected,
            new CombinatorialDataAttribute()
                .GetData(typeof(CombinatorialRangeTests).GetMethod(test)!)
                .Select(row => row[0]));

    [TestMethod]
    public void An_invalid_range_is_rejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CombinatorialRangeAttribute(1, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CombinatorialRangeAttribute(1, 3, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CombinatorialRangeAttribute(3, 1, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CombinatorialRangeAttribute(1, 3, -1));
    }
}
