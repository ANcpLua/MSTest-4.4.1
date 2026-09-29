using System.Collections;

namespace MSTest.Assertions.CSharp12.SourceCompatibility;

/// <summary>
/// A collection that converts two ways: it is a sequence of its tracks and converts implicitly to a span of them.
/// </summary>
internal sealed class Playlist(params string[] tracks) : IEnumerable<string>
{
    public static implicit operator Span<string>(Playlist playlist) => playlist.Tracks;

    public static implicit operator ReadOnlySpan<string>(Playlist playlist) => playlist.Tracks;

    private string[] Tracks { get; } = tracks;

    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)Tracks).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
