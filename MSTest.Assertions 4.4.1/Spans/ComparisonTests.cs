namespace MSTest.Assertions.Spans;

internal sealed class Reading(string sensor, double value)
{
    public string Sensor { get; } = sensor;

    public double Value { get; } = value;
}

internal sealed class CalibratedReading(string sensor, double value, double offset)
{
    public string Sensor { get; } = sensor;

    public double Value { get; } = value;

    public double Offset { get; } = offset;
}

/// <summary>
/// Compare two spans or two memory regions: element by element with <c>AreSequenceEqual</c>,
/// member by member with <c>AreEquivalent</c>.
/// </summary>
[TestClass]
public sealed class ComparisonTests
{
    private static readonly int[] Samples = [1, 2, 3];
    private static readonly int[] SameSamples = [1, 2, 3];
    private static readonly int[] ReversedSamples = [3, 2, 1];
    private static readonly int[] OtherSamples = [1, 2, 4];

    private static readonly string[] Codes = ["eur", "usd"];
    private static readonly string[] UpperCodes = ["EUR", "USD"];
    private static readonly string[] ReversedUpperCodes = ["USD", "EUR"];

    private static readonly Reading[] Readings = [new("inlet", 20.5), new("outlet", 61.0)];
    private static readonly Reading[] SameReadings = [new("inlet", 20.5), new("outlet", 61.0)];
    private static readonly Reading[] OtherReadings = [new("inlet", 20.5), new("outlet", 64.0)];

    private static readonly object[] Expected = [new Reading("inlet", 20.5)];
    private static readonly object[] Calibrated = [new CalibratedReading("inlet", 20.5, 0.1)];

    [TestMethod]
    public void AreSequenceEqual_passes_for_equal_elements_in_the_same_order()
    {
        Assert.AreSequenceEqual(new Span<int>(Samples), new Span<int>(SameSamples));
        Assert.AreSequenceEqual(new ReadOnlySpan<int>(Samples), new ReadOnlySpan<int>(SameSamples));
        Assert.AreSequenceEqual(new Memory<int>(Samples), new Memory<int>(SameSamples));
        Assert.AreSequenceEqual(new ReadOnlyMemory<int>(Samples), new ReadOnlyMemory<int>(SameSamples));
    }

    [TestMethod]
    public void AreSequenceEqual_fails_for_another_order()
    {
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreSequenceEqual(new Span<int>(Samples), new Span<int>(ReversedSamples)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreSequenceEqual(new ReadOnlySpan<int>(Samples), new ReadOnlySpan<int>(ReversedSamples)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreSequenceEqual(new Memory<int>(Samples), new Memory<int>(ReversedSamples)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreSequenceEqual(new ReadOnlyMemory<int>(Samples), new ReadOnlyMemory<int>(ReversedSamples)));
    }

    [TestMethod]
    public void AreSequenceEqual_takes_an_order_and_a_comparer()
    {
        Assert.AreSequenceEqual(new Span<int>(Samples), new Span<int>(ReversedSamples), SequenceOrder.InAnyOrder);
        Assert.AreSequenceEqual(
            new ReadOnlySpan<string>(Codes), new ReadOnlySpan<string>(UpperCodes), StringComparer.OrdinalIgnoreCase);
        Assert.AreSequenceEqual(
            new ReadOnlyMemory<string>(Codes),
            new ReadOnlyMemory<string>(ReversedUpperCodes),
            StringComparer.OrdinalIgnoreCase,
            SequenceOrder.InAnyOrder);

        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreSequenceEqual(new Memory<int>(Samples), new Memory<int>(OtherSamples), SequenceOrder.InAnyOrder));
    }

    [TestMethod]
    public void AreNotSequenceEqual_passes_for_another_order()
    {
        Assert.AreNotSequenceEqual(new Span<int>(Samples), new Span<int>(ReversedSamples));
        Assert.AreNotSequenceEqual(new ReadOnlySpan<int>(Samples), new ReadOnlySpan<int>(ReversedSamples));
        Assert.AreNotSequenceEqual(new Memory<int>(Samples), new Memory<int>(ReversedSamples));
        Assert.AreNotSequenceEqual(new ReadOnlyMemory<int>(Samples), new ReadOnlyMemory<int>(ReversedSamples));
    }

    [TestMethod]
    public void AreNotSequenceEqual_fails_for_equal_elements_in_the_same_order()
    {
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotSequenceEqual(new Span<int>(Samples), new Span<int>(SameSamples)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotSequenceEqual(new ReadOnlySpan<int>(Samples), new ReadOnlySpan<int>(SameSamples)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotSequenceEqual(new Memory<int>(Samples), new Memory<int>(SameSamples)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotSequenceEqual(new ReadOnlyMemory<int>(Samples), new ReadOnlyMemory<int>(SameSamples)));
    }

    [TestMethod]
    public void AreNotSequenceEqual_takes_an_order_and_a_comparer()
    {
        Assert.AreNotSequenceEqual(new Span<int>(Samples), new Span<int>(OtherSamples), SequenceOrder.InAnyOrder);

        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotSequenceEqual(
                new ReadOnlySpan<string>(Codes), new ReadOnlySpan<string>(UpperCodes), StringComparer.OrdinalIgnoreCase));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotSequenceEqual(new Memory<int>(Samples), new Memory<int>(ReversedSamples), SequenceOrder.InAnyOrder));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotSequenceEqual(
                new ReadOnlyMemory<string>(Codes),
                new ReadOnlyMemory<string>(ReversedUpperCodes),
                StringComparer.OrdinalIgnoreCase,
                SequenceOrder.InAnyOrder));
    }

    // Reading has no equality of its own: two instances are equivalent when their public members are.
    [TestMethod]
    public void AreEquivalent_passes_for_elements_with_equal_members()
    {
        Assert.AreEquivalent(new Span<Reading>(Readings), new Span<Reading>(SameReadings));
        Assert.AreEquivalent(new ReadOnlySpan<Reading>(Readings), new ReadOnlySpan<Reading>(SameReadings));
        Assert.AreEquivalent(new Memory<Reading>(Readings), new Memory<Reading>(SameReadings));
        Assert.AreEquivalent(new ReadOnlyMemory<Reading>(Readings), new ReadOnlyMemory<Reading>(SameReadings));

        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreSequenceEqual(new ReadOnlySpan<Reading>(Readings), new ReadOnlySpan<Reading>(SameReadings)));
    }

    [TestMethod]
    public void AreEquivalent_fails_for_a_member_that_differs()
    {
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreEquivalent(new Span<Reading>(Readings), new Span<Reading>(OtherReadings)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreEquivalent(new ReadOnlySpan<Reading>(Readings), new ReadOnlySpan<Reading>(OtherReadings)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreEquivalent(new Memory<Reading>(Readings), new Memory<Reading>(OtherReadings)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreEquivalent(new ReadOnlyMemory<Reading>(Readings), new ReadOnlyMemory<Reading>(OtherReadings)));
    }

    // Strict mode rejects members that only the actual element declares.
    [TestMethod]
    public void AreEquivalent_takes_the_strict_mode()
    {
        Assert.AreEquivalent(new ReadOnlySpan<object>(Expected), new ReadOnlySpan<object>(Calibrated), strict: false);
        Assert.AreEquivalent(new Memory<object>(Expected), new Memory<object>(Calibrated), strict: false);

        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreEquivalent(new Span<object>(Expected), new Span<object>(Calibrated), strict: true));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreEquivalent(new ReadOnlyMemory<object>(Expected), new ReadOnlyMemory<object>(Calibrated), strict: true));
    }

    [TestMethod]
    public void AreNotEquivalent_passes_for_a_member_that_differs()
    {
        Assert.AreNotEquivalent(new Span<Reading>(Readings), new Span<Reading>(OtherReadings));
        Assert.AreNotEquivalent(new ReadOnlySpan<Reading>(Readings), new ReadOnlySpan<Reading>(OtherReadings));
        Assert.AreNotEquivalent(new Memory<Reading>(Readings), new Memory<Reading>(OtherReadings));
        Assert.AreNotEquivalent(new ReadOnlyMemory<Reading>(Readings), new ReadOnlyMemory<Reading>(OtherReadings));
    }

    [TestMethod]
    public void AreNotEquivalent_fails_for_elements_with_equal_members()
    {
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotEquivalent(new Span<Reading>(Readings), new Span<Reading>(SameReadings)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotEquivalent(new ReadOnlySpan<Reading>(Readings), new ReadOnlySpan<Reading>(SameReadings)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotEquivalent(new Memory<Reading>(Readings), new Memory<Reading>(SameReadings)));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotEquivalent(new ReadOnlyMemory<Reading>(Readings), new ReadOnlyMemory<Reading>(SameReadings)));
    }

    [TestMethod]
    public void AreNotEquivalent_takes_the_strict_mode()
    {
        Assert.AreNotEquivalent(new Span<object>(Expected), new Span<object>(Calibrated), strict: true);
        Assert.AreNotEquivalent(new ReadOnlyMemory<object>(Expected), new ReadOnlyMemory<object>(Calibrated), strict: true);

        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotEquivalent(new ReadOnlySpan<object>(Expected), new ReadOnlySpan<object>(Calibrated), strict: false));
        Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.AreNotEquivalent(new Memory<object>(Expected), new Memory<object>(Calibrated), strict: false));
    }
}
