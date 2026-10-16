namespace MSTest.Assertions.Spans;

/// <summary>
///     Look for elements in a span or a memory region: one value, a match for a predicate, a set of values,
///     or exactly one element, which <c>ContainsSingle</c> returns.
/// </summary>
[TestClass]
public sealed class ContainmentTests
{
    private static readonly int[] Codes = [200, 204, 404];
    private static readonly int[] Success = [200, 204];
    private static readonly int[] SuccessAndFailure = [200, 500];
    private static readonly int[] OnlyCode = [200];

    private static readonly string[] Methods = ["get", "put"];
    private static readonly string[] UpperMethods = ["GET", "PUT"];

    [TestMethod]
    public void Contains_passes_for_a_present_value()
    {
        Assert.Contains(404, new Span<int>(Codes));
        Assert.Contains(404, new ReadOnlySpan<int>(Codes));
        Assert.Contains(404, new Memory<int>(Codes));
        Assert.Contains(404, new ReadOnlyMemory<int>(Codes));
    }

    [TestMethod]
    public void Contains_fails_for_a_missing_value()
    {
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains(500, new Span<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains(500, new ReadOnlySpan<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains(500, new Memory<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains(500, new ReadOnlyMemory<int>(Codes)));
    }

    [TestMethod]
    public void Contains_takes_a_comparer_or_a_predicate()
    {
        Assert.Contains("GET", new ReadOnlySpan<string>(Methods), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("GET", new Span<string>(Methods), StringComparer.OrdinalIgnoreCase);
        Assert.Contains(code => code >= 400, new Memory<int>(Codes));

        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.Contains(code => code >= 500, new ReadOnlyMemory<int>(Codes)));
    }

    [TestMethod]
    public void DoesNotContain_passes_for_a_missing_value()
    {
        Assert.DoesNotContain(500, new Span<int>(Codes));
        Assert.DoesNotContain(500, new ReadOnlySpan<int>(Codes));
        Assert.DoesNotContain(500, new Memory<int>(Codes));
        Assert.DoesNotContain(500, new ReadOnlyMemory<int>(Codes));
    }

    [TestMethod]
    public void DoesNotContain_fails_for_a_present_value()
    {
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain(404, new Span<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain(404, new ReadOnlySpan<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain(404, new Memory<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain(404, new ReadOnlyMemory<int>(Codes)));
    }

    [TestMethod]
    public void DoesNotContain_takes_a_comparer_or_a_predicate()
    {
        Assert.DoesNotContain(code => code >= 500, new ReadOnlyMemory<int>(Codes));

        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.DoesNotContain("GET", new Span<string>(Methods), StringComparer.OrdinalIgnoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.DoesNotContain("GET", new ReadOnlySpan<string>(Methods), StringComparer.OrdinalIgnoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.DoesNotContain(code => code >= 400, new Memory<int>(Codes)));
    }

    [TestMethod]
    public void ContainsAll_passes_when_every_expected_value_is_present()
    {
        Assert.ContainsAll(new Span<int>(Success), new Span<int>(Codes));
        Assert.ContainsAll(new ReadOnlySpan<int>(Success), new ReadOnlySpan<int>(Codes));
        Assert.ContainsAll(new Memory<int>(Success), new Memory<int>(Codes));
        Assert.ContainsAll(new ReadOnlyMemory<int>(Success), new ReadOnlyMemory<int>(Codes));
    }

    [TestMethod]
    public void ContainsAll_fails_when_one_expected_value_is_missing()
    {
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.ContainsAll(new Span<int>(SuccessAndFailure), new Span<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.ContainsAll(new ReadOnlySpan<int>(SuccessAndFailure), new ReadOnlySpan<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.ContainsAll(new Memory<int>(SuccessAndFailure), new Memory<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.ContainsAll(new ReadOnlyMemory<int>(SuccessAndFailure), new ReadOnlyMemory<int>(Codes)));
    }

    [TestMethod]
    public void ContainsAll_takes_a_comparer()
    {
        Assert.ContainsAll(
            new ReadOnlySpan<string>(UpperMethods), new ReadOnlySpan<string>(Methods),
            StringComparer.OrdinalIgnoreCase);
        Assert.ContainsAll(new Memory<string>(UpperMethods), new Memory<string>(Methods),
            StringComparer.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void DoesNotContainAll_passes_when_one_value_is_missing()
    {
        Assert.DoesNotContainAll(new Span<int>(SuccessAndFailure), new Span<int>(Codes));
        Assert.DoesNotContainAll(new ReadOnlySpan<int>(SuccessAndFailure), new ReadOnlySpan<int>(Codes));
        Assert.DoesNotContainAll(new Memory<int>(SuccessAndFailure), new Memory<int>(Codes));
        Assert.DoesNotContainAll(new ReadOnlyMemory<int>(SuccessAndFailure), new ReadOnlyMemory<int>(Codes));
    }

    [TestMethod]
    public void DoesNotContainAll_fails_when_every_value_is_present()
    {
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.DoesNotContainAll(new Span<int>(Success), new Span<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.DoesNotContainAll(new ReadOnlySpan<int>(Success), new ReadOnlySpan<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.DoesNotContainAll(new Memory<int>(Success), new Memory<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.DoesNotContainAll(new ReadOnlyMemory<int>(Success), new ReadOnlyMemory<int>(Codes)));
    }

    [TestMethod]
    public void DoesNotContainAll_takes_a_comparer()
    {
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContainAll(new Span<string>(UpperMethods),
            new Span<string>(Methods), StringComparer.OrdinalIgnoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContainAll(
            new ReadOnlyMemory<string>(UpperMethods), new ReadOnlyMemory<string>(Methods),
            StringComparer.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void ContainsSingle_returns_the_only_element()
    {
        Assert.AreEqual(200, Assert.ContainsSingle(new Span<int>(OnlyCode)));
        Assert.AreEqual(200, Assert.ContainsSingle(new ReadOnlySpan<int>(OnlyCode)));
        Assert.AreEqual(200, Assert.ContainsSingle(new Memory<int>(OnlyCode)));
        Assert.AreEqual(200, Assert.ContainsSingle(new ReadOnlyMemory<int>(OnlyCode)));
    }

    [TestMethod]
    public void ContainsSingle_fails_for_several_elements()
    {
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsSingle(new Span<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsSingle(new ReadOnlySpan<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsSingle(new Memory<int>(Codes)));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsSingle(new ReadOnlyMemory<int>(Codes)));
    }

    [TestMethod]
    public void ContainsSingle_returns_the_only_match_for_a_predicate()
    {
        Assert.AreEqual(404, Assert.ContainsSingle(code => code >= 400, new Span<int>(Codes)));
        Assert.AreEqual(404, Assert.ContainsSingle(code => code >= 400, new ReadOnlySpan<int>(Codes)));
        Assert.AreEqual(404, Assert.ContainsSingle(code => code >= 400, new Memory<int>(Codes)));
        Assert.AreEqual(404, Assert.ContainsSingle(code => code >= 400, new ReadOnlyMemory<int>(Codes)));

        Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.ContainsSingle(code => code < 400, new ReadOnlySpan<int>(Codes)));
    }

    // An interpolated message is formatted only when the assertion fails.
    [TestMethod]
    public void Interpolated_message_is_formatted_on_failure_only()
    {
        var formatted = 0;

        Assert.AreEqual(200, Assert.ContainsSingle(new Span<int>(OnlyCode), $"ambiguous: {Count()}"));
        Assert.AreEqual(0, formatted);

        var failure = Assert.ThrowsExactly<AssertFailedException>(() =>
            Assert.ContainsSingle(new ReadOnlyMemory<int>(Codes), $"ambiguous: {Count()}"));
        Assert.Contains("ambiguous: 3 codes, formatted 1 time(s)", failure.Message);
        return;

        string Count()
        {
            return $"{Codes.Length} codes, formatted {++formatted} time(s)";
        }
    }
}