using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// What <see cref="SpanFileExporter"/> keeps of a finished span: its name, its place in the trace, its status,
/// its attributes, its events and the attributes of the resource that the provider attaches to every span,
/// written as one line of JSON.
/// </summary>
internal sealed record Span(
    string Name,
    string TraceId,
    string SpanId,
    string ParentSpanId,
    string? TraceState,
    ActivityStatusCode Status,
    IReadOnlyDictionary<string, string?> Attributes,
    IReadOnlyList<SpanEvent> Events,
    IReadOnlyDictionary<string, string?> Resource)
{
    public static Span From(Activity activity, IEnumerable<KeyValuePair<string, object>> resource) => new(
        activity.DisplayName,
        activity.TraceId.ToHexString(),
        activity.SpanId.ToHexString(),
        activity.ParentSpanId.ToHexString(),
        activity.TraceStateString,
        activity.Status,
        activity.TagObjects.ToDictionary(static tag => tag.Key, static tag => Text(tag.Value)),
        [.. activity.Events.Select(static occurrence => new SpanEvent(
            occurrence.Name,
            occurrence.Tags.ToDictionary(static tag => tag.Key, static tag => Text(tag.Value))))],
        resource.ToDictionary(static attribute => attribute.Key, static attribute => Text(attribute.Value)));

    public static Span Parse(string line) => JsonSerializer.Deserialize<Span>(line)!;

    public string ToLine() => JsonSerializer.Serialize(this);

    private static string? Text(object? value) => Convert.ToString(value, CultureInfo.InvariantCulture);
}

/// <summary>An event of a span, such as the <c>exception</c> event of a failed test, with its attributes.</summary>
internal sealed record SpanEvent(string Name, IReadOnlyDictionary<string, string?> Attributes);
