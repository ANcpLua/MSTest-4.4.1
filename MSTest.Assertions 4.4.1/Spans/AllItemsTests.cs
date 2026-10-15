namespace MSTest.Assertions.Spans;

/// <summary>
///     Check every element of a span or a memory region: all distinct, none null, all of one type.
/// </summary>
[TestClass]
public sealed class AllItemsTests
{
    private static readonly int[] Ports = [80, 443, 8080];
    private static readonly int[] PortsWithDuplicate = [80, 443, 80];

    private static readonly string[] Hosts = ["api", "API"];

    private static readonly string?[] Names = ["alpha", "beta"];
    private static readonly string?[] NamesWithGap = ["alpha", null];

    private static readonly object[] Texts = ["alpha", "beta"];
    private static readonly object[] TextsAndNumbers = ["alpha", 2];

    [TestMethod]
    public void AreAllDistinct_passes_without_duplicates()
    {
        Assert.AreAllDistinct(new Span<int>(Ports));
        Assert.AreAllDistinct(new ReadOnlySpan<int>(Ports));
        Assert.AreAllDistinct(new Memory<int>(Ports));
        Assert.AreAllDistinct(new ReadOnlyMemory<int>(Ports));
    }

    [TestMethod]
    public void AreAllDistinct_fails_with_a_duplicate()
    {
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllDistinct(new Span<int>(PortsWithDuplicate)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllDistinct(new ReadOnlySpan<int>(PortsWithDuplicate)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllDistinct(new Memory<int>(PortsWithDuplicate)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllDistinct(new ReadOnlyMemory<int>(PortsWithDuplicate)));
    }

    [TestMethod]
    public void AreAllDistinct_takes_a_comparer()
    {
        Assert.AreAllDistinct(new ReadOnlySpan<string>(Hosts));

        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllDistinct(new ReadOnlySpan<string>(Hosts), StringComparer.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void AreAllNotNull_passes_without_null()
    {
        Assert.AreAllNotNull(new Span<string?>(Names));
        Assert.AreAllNotNull(new ReadOnlySpan<string?>(Names));
        Assert.AreAllNotNull(new Memory<string?>(Names));
        Assert.AreAllNotNull(new ReadOnlyMemory<string?>(Names));
    }

    [TestMethod]
    public void AreAllNotNull_fails_with_null()
    {
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllNotNull(new Span<string?>(NamesWithGap)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllNotNull(new ReadOnlySpan<string?>(NamesWithGap)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllNotNull(new Memory<string?>(NamesWithGap)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllNotNull(new ReadOnlyMemory<string?>(NamesWithGap)));
    }

    [TestMethod]
    public void AreAllOfType_passes_when_every_element_has_the_type()
    {
        Assert.AreAllOfType<string, object>(new Span<object>(Texts));
        Assert.AreAllOfType<string, object>(new ReadOnlySpan<object>(Texts));
        Assert.AreAllOfType<string, object>(new Memory<object>(Texts));
        Assert.AreAllOfType<string, object>(new ReadOnlyMemory<object>(Texts));
    }

    [TestMethod]
    public void AreAllOfType_fails_when_one_element_has_another_type()
    {
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllOfType<string, object>(new Span<object>(TextsAndNumbers)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllOfType<string, object>(new ReadOnlySpan<object>(TextsAndNumbers)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllOfType<string, object>(new Memory<object>(TextsAndNumbers)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllOfType<string, object>(new ReadOnlyMemory<object>(TextsAndNumbers)));
    }

    // The type is only known at run time: pass it as the first argument.
    [TestMethod]
    public void AreAllOfType_takes_a_runtime_type()
    {
        var expected = typeof(string);

        Assert.AreAllOfType(expected, new Span<object>(Texts));
        Assert.AreAllOfType(expected, new ReadOnlySpan<object>(Texts));
        Assert.AreAllOfType(expected, new Memory<object>(Texts));
        Assert.AreAllOfType(expected, new ReadOnlyMemory<object>(Texts));

        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.AreAllOfType(expected, new ReadOnlySpan<object>(TextsAndNumbers)));
    }
}