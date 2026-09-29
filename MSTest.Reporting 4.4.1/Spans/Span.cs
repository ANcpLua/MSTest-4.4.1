using System.Diagnostics;
using System.Text.Json;

namespace MSTest.Reporting.Spans;

/// <summary>
/// What <see cref="SpanFileExporter"/> keeps of a finished span: its name, its place in the trace, its status,
/// its attributes and its events, written as one line of JSON.
/// </summary>
internal sealed record Span(
    string Name,
    string TraceId,
    string SpanId,
    string ParentSpanId,
    ActivityStatusCode Status,
    IReadOnlyDictionary<string, string?> Attributes,
    IReadOnlyList<SpanEvent> Events)
{
    public static Span From(Activity activity) => new(
        activity.DisplayName,
        activity.TraceId.ToHexString(),
        activity.SpanId.ToHexString(),
        activity.ParentSpanId.ToHexString(),
        activity.Status,
        activity.TagObjects.ToDictionary(static tag => tag.Key, static tag => tag.Value?.ToString()),
        [.. activity.Events.Select(static occurrence => new SpanEvent(
            occurrence.Name,
            occurrence.Tags.ToDictionary(static tag => tag.Key, static tag => tag.Value?.ToString())))]);

    public static Span Parse(string line) => JsonSerializer.Deserialize<Span>(line)!;

    public string ToLine() => JsonSerializer.Serialize(this);
}

/// <summary>An event of a span, such as the <c>exception</c> event of a failed test, with its attributes.</summary>
internal sealed record SpanEvent(string Name, IReadOnlyDictionary<string, string?> Attributes);
