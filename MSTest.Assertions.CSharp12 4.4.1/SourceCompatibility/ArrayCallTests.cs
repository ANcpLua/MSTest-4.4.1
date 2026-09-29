namespace MSTest.Assertions.CSharp12.SourceCompatibility;

/// <summary>
/// Pass arrays to the collection assertions in a project that compiles as C# 12. The calls bind to the overloads
/// that take an array, whether the element type is written out or inferred from another argument.
/// </summary>
[TestClass]
public sealed class ArrayCallTests
{
    private static readonly int[] Codes = [200, 204, 404];
    private static readonly int[] SameCodes = [200, 204, 404];
    private static readonly int[] ReversedCodes = [404, 204, 200];
    private static readonly int[] Success = [200, 204];
    private static readonly int[] OnlyCode = [200];
    private static readonly int[] NoCodes = [];

    private static readonly string[] Methods = ["get", "put"];
    private static readonly string[] UpperMethods = ["GET", "PUT"];
    private static readonly string[] SameMethodTwice = ["get", "GET"];
    private static readonly string[] Retries = ["get", "get"];
    private static readonly string?[] MethodsWithGap = ["get", null];

    [TestMethod]
    public void Element_type_inferred_from_the_expected_value()
    {
        Assert.Contains(404, Codes);
        Assert.DoesNotContain(500, Codes);

        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains(500, Codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain(404, Codes));
    }

    [TestMethod]
    public void Element_type_inferred_from_the_predicate()
    {
        Func<int, bool> isError = code => code >= 400;
        Func<int, bool> isServerError = code => code >= 500;

        Assert.Contains(isError, Codes);
        Assert.DoesNotContain(isServerError, Codes);
        Assert.AreEqual(404, Assert.ContainsSingle(isError, Codes));

        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains(isServerError, Codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain(isError, Codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsSingle(isServerError, Codes));
    }

    [TestMethod]
    public void Element_type_inferred_from_the_comparer()
    {
        StringComparer ignoreCase = StringComparer.OrdinalIgnoreCase;

        Assert.AreAllDistinct(Methods, ignoreCase);
        Assert.Contains("GET", Methods, ignoreCase);
        Assert.DoesNotContain("POST", Methods, ignoreCase);
        Assert.ContainsAll(UpperMethods, Methods, ignoreCase);
        Assert.DoesNotContainAll(SameMethodTwice, Methods, ignoreCase);
        Assert.AreSequenceEqual(UpperMethods, Methods, ignoreCase);
        Assert.AreSequenceEqual(UpperMethods, Methods, ignoreCase, SequenceOrder.InAnyOrder);
        Assert.AreNotSequenceEqual(SameMethodTwice, Methods, ignoreCase);
        Assert.AreNotSequenceEqual(SameMethodTwice, Methods, ignoreCase, SequenceOrder.InAnyOrder);

        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllDistinct(SameMethodTwice, ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains("POST", Methods, ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain("GET", Methods, ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsAll(SameMethodTwice, Methods, ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContainAll(UpperMethods, Methods, ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreSequenceEqual(SameMethodTwice, Methods, ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreNotSequenceEqual(UpperMethods, Methods, ignoreCase));
    }

    [TestMethod]
    public void Element_type_written_out()
    {
        Assert.HasCount<int>(3, Codes);
        Assert.IsNotEmpty<int>(Codes);
        Assert.IsEmpty<int>(NoCodes);
        Assert.AreAllDistinct<int>(Codes);
        Assert.AreAllNotNull<string>(Methods);
        Assert.ContainsAll<int>(Success, Codes);
        Assert.DoesNotContainAll<int>(Codes, Success);
        Assert.AreSequenceEqual<int>(SameCodes, Codes);
        Assert.AreSequenceEqual<int>(ReversedCodes, Codes, SequenceOrder.InAnyOrder);
        Assert.AreNotSequenceEqual<int>(ReversedCodes, Codes);
        Assert.AreNotSequenceEqual<int>(Success, Codes, SequenceOrder.InAnyOrder);
        Assert.AreEqual(200, Assert.ContainsSingle<int>(OnlyCode));

        Assert.ThrowsExactly<AssertFailedException>(() => Assert.HasCount<int>(2, Codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsNotEmpty<int>(NoCodes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.IsEmpty<int>(Codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllDistinct<string>(Retries));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllNotNull<string?>(MethodsWithGap));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsAll<int>(Codes, Success));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContainAll<int>(Success, Codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreSequenceEqual<int>(ReversedCodes, Codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreNotSequenceEqual<int>(SameCodes, Codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsSingle<int>(Codes));
    }

    // An interpolated message next to a written-out element type is formatted only when the assertion fails.
    [TestMethod]
    public void Interpolated_message_is_formatted_on_failure_only()
    {
        int formatted = 0;
        string Count() => $"{Codes.Length} codes, formatted {++formatted} time(s)";

        Assert.HasCount<int>(3, Codes, $"unexpected: {Count()}");
        Assert.IsNotEmpty<int>(Codes, $"unexpected: {Count()}");
        Assert.IsEmpty<int>(NoCodes, $"unexpected: {Count()}");
        Assert.AreEqual(200, Assert.ContainsSingle<int>(OnlyCode, $"unexpected: {Count()}"));
        Assert.AreEqual(0, formatted);

        AssertFailedException failure = Assert.ThrowsExactly<AssertFailedException>(
            () => Assert.HasCount<int>(2, Codes, $"unexpected: {Count()}"));
        Assert.Contains("unexpected: 3 codes, formatted 1 time(s)", failure.Message);
    }

    // An array next to a list binds to the overloads that take an array and an IEnumerable<T>.
    [TestMethod]
    public void Array_next_to_a_sequence()
    {
        List<int> received = [200, 204, 404];

        Assert.AreSequenceEqual(SameCodes, received);
        Assert.AreSequenceEqual(ReversedCodes, received, SequenceOrder.InAnyOrder);
        Assert.AreNotSequenceEqual(ReversedCodes, received);
        Assert.AreNotSequenceEqual(Success, received, SequenceOrder.InAnyOrder);
        Assert.ContainsAll(Success, received);

        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreSequenceEqual(ReversedCodes, received));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreNotSequenceEqual(SameCodes, received));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContainAll(Success, received));
    }
}
