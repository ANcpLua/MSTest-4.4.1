namespace MSTest.Assertions.Spans;

/// <summary>
/// Assert that a span or a memory region is empty, or is not, without copying it into a collection first.
/// </summary>
[TestClass]
public sealed class EmptinessTests
{
    private static readonly byte[] Payload = [0x4D, 0x53, 0x54];

    [TestMethod]
    public void IsEmpty_passes_without_elements()
    {
        Assert.IsEmpty(Span<byte>.Empty);
        Assert.IsEmpty(ReadOnlySpan<byte>.Empty);
        Assert.IsEmpty(Memory<byte>.Empty);
        Assert.IsEmpty(ReadOnlyMemory<byte>.Empty);
    }

    [TestMethod]
    public void IsEmpty_fails_with_elements()
    {
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsEmpty(new Span<byte>(Payload)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsEmpty(new ReadOnlySpan<byte>(Payload)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsEmpty(new Memory<byte>(Payload)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsEmpty(new ReadOnlyMemory<byte>(Payload)));
    }

    [TestMethod]
    public void IsNotEmpty_passes_with_elements()
    {
        Assert.IsNotEmpty(new Span<byte>(Payload));
        Assert.IsNotEmpty(new ReadOnlySpan<byte>(Payload));
        Assert.IsNotEmpty(new Memory<byte>(Payload));
        Assert.IsNotEmpty(new ReadOnlyMemory<byte>(Payload));
    }

    [TestMethod]
    public void IsNotEmpty_fails_without_elements()
    {
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsNotEmpty(Span<byte>.Empty));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsNotEmpty(ReadOnlySpan<byte>.Empty));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsNotEmpty(Memory<byte>.Empty));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsNotEmpty(ReadOnlyMemory<byte>.Empty));
    }

    // An interpolated message is formatted only when the assertion fails.
    [TestMethod]
    public void Interpolated_message_is_formatted_on_failure_only()
    {
        int formatted = 0;
        string Length() => $"{Payload.Length} bytes, formatted {++formatted} time(s)";

        Assert.IsEmpty(Span<byte>.Empty, $"left over: {Length()}");
        Assert.IsNotEmpty(new Memory<byte>(Payload), $"nothing received: {Length()}");
        Assert.AreEqual(0, formatted);

        AssertFailedException failure = Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.IsEmpty(new ReadOnlyMemory<byte>(Payload), $"left over: {Length()}"));
        Assert.Contains("left over: 3 bytes, formatted 1 time(s)", failure.Message);
    }
}
