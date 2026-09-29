namespace MSTest.Assertions.CSharp12.SourceCompatibility;

/// <summary>
/// Pass collections that are a sequence and convert to a span as well, in a project that compiles as C# 12:
/// a string as its characters, an <see cref="ArraySegment{T}"/>, a type with its own conversions.
/// The calls bind to the overloads that take the type of the collection as a type argument.
/// </summary>
[TestClass]
public sealed class DualConvertibleCallTests
{
    private static readonly int[] Buffer = [0, 200, 204, 404, 0];

    [TestMethod]
    public void String_as_its_characters()
    {
        const string Code = "A7C";
        Func<char, bool> isDigit = char.IsDigit;
        Func<char, bool> isLower = char.IsLower;
        EqualityComparer<char> characters = EqualityComparer<char>.Default;

        Assert.Contains('7', Code);
        Assert.Contains('7', Code, characters);
        Assert.Contains(isDigit, Code);
        Assert.DoesNotContain('B', Code);
        Assert.DoesNotContain('B', Code, characters);
        Assert.DoesNotContain(isLower, Code);
        Assert.AreEqual('7', Assert.ContainsSingle(isDigit, Code));
        Assert.AreAllDistinct(Code, characters);
        Assert.ContainsAll("CA", Code, characters);
        Assert.DoesNotContainAll("AB", Code, characters);
        Assert.AreSequenceEqual("A7C", Code, characters);
        Assert.AreSequenceEqual("C7A", Code, characters, SequenceOrder.InAnyOrder);
        Assert.AreNotSequenceEqual("C7A", Code, characters);
        Assert.AreNotSequenceEqual("A7D", Code, characters, SequenceOrder.InAnyOrder);

        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains('B', Code));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain(isDigit, Code));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsSingle(isLower, Code));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllDistinct("A7A", characters));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsAll("AB", Code, characters));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContainAll("CA", Code, characters));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreSequenceEqual("C7A", Code, characters));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreNotSequenceEqual("A7C", Code, characters));
    }

    [TestMethod]
    public void Array_segment()
    {
        ArraySegment<int> codes = new(Buffer, offset: 1, count: 3);
        ArraySegment<int> success = new(Buffer, offset: 1, count: 2);
        ArraySegment<int> padding = new(Buffer, offset: 0, count: 2);
        Func<int, bool> isError = code => code >= 400;
        Func<int, bool> isServerError = code => code >= 500;
        EqualityComparer<int> numbers = EqualityComparer<int>.Default;

        Assert.Contains(404, codes);
        Assert.Contains(404, codes, numbers);
        Assert.Contains(isError, codes);
        Assert.DoesNotContain(0, codes);
        Assert.DoesNotContain(0, codes, numbers);
        Assert.DoesNotContain(isServerError, codes);
        Assert.AreEqual(404, Assert.ContainsSingle(isError, codes));
        Assert.AreAllDistinct(codes, numbers);
        Assert.ContainsAll(success, codes, numbers);
        Assert.DoesNotContainAll(padding, codes, numbers);
        Assert.AreSequenceEqual(codes, codes, numbers);
        Assert.AreSequenceEqual(codes, codes, numbers, SequenceOrder.InAnyOrder);
        Assert.AreNotSequenceEqual(success, codes, numbers);
        Assert.AreNotSequenceEqual(success, codes, numbers, SequenceOrder.InAnyOrder);

        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains(0, codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain(isError, codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsSingle(isServerError, codes));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllDistinct(new ArraySegment<int>(Buffer), numbers));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsAll(padding, codes, numbers));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContainAll(success, codes, numbers));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreSequenceEqual(success, codes, numbers));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreNotSequenceEqual(codes, codes, numbers));
    }

    [TestMethod]
    public void Type_with_its_own_conversions()
    {
        Playlist playlist = new("intro", "theme", "outro");
        Playlist sameTracks = new("INTRO", "THEME", "OUTRO");
        Playlist opening = new("intro", "theme");
        Playlist encore = new("theme", "reprise");
        Func<string, bool> startsWithT = track => track.StartsWith('t');
        Func<string, bool> startsWithR = track => track.StartsWith('r');
        StringComparer ignoreCase = StringComparer.OrdinalIgnoreCase;

        Assert.Contains("theme", playlist);
        Assert.Contains("THEME", playlist, ignoreCase);
        Assert.Contains(startsWithT, playlist);
        Assert.DoesNotContain("reprise", playlist);
        Assert.DoesNotContain("REPRISE", playlist, ignoreCase);
        Assert.DoesNotContain(startsWithR, playlist);
        Assert.AreEqual("theme", Assert.ContainsSingle(startsWithT, playlist));
        Assert.AreAllDistinct(playlist, ignoreCase);
        Assert.ContainsAll(opening, playlist, ignoreCase);
        Assert.DoesNotContainAll(encore, playlist, ignoreCase);
        Assert.AreSequenceEqual(sameTracks, playlist, ignoreCase);
        Assert.AreSequenceEqual(sameTracks, playlist, ignoreCase, SequenceOrder.InAnyOrder);
        Assert.AreNotSequenceEqual(opening, playlist, ignoreCase);
        Assert.AreNotSequenceEqual(opening, playlist, ignoreCase, SequenceOrder.InAnyOrder);

        Assert.ThrowsExactly<AssertFailedException>(() => Assert.Contains("reprise", playlist));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContain(startsWithT, playlist));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsSingle(startsWithR, playlist));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreAllDistinct(new Playlist("intro", "INTRO"), ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.ContainsAll(encore, playlist, ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.DoesNotContainAll(opening, playlist, ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreSequenceEqual(opening, playlist, ignoreCase));
        Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreNotSequenceEqual(sameTracks, playlist, ignoreCase));
    }
}
