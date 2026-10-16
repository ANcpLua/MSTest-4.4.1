using System.Globalization;
using System.Text.Json;
using OpenTelemetry.Metrics;

namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// What <see cref="InstrumentFileExporter"/> keeps of one series of an instrument: the name, the unit and the
/// kind of the instrument and the attributes that group the series, written as one line of JSON.
/// </summary>
internal sealed record Instrument(string Name, string Unit, MetricType Kind, IReadOnlyDictionary<string, string?> Attributes, long? Value, long? Count, double? Sum)
{
    public static Instrument From(Metric metric, in MetricPoint series)
    {
        var attributes = new Dictionary<string, string?>();
        foreach (var attribute in series.Tags)
        {
            attributes[attribute.Key] = Convert.ToString(attribute.Value, CultureInfo.InvariantCulture);
        }

        return new Instrument(metric.Name, metric.Unit, metric.MetricType, attributes,
            metric.MetricType is MetricType.LongSum or MetricType.LongSumNonMonotonic ? series.GetSumLong() : null,
            metric.MetricType == MetricType.Histogram ? series.GetHistogramCount() : null,
            metric.MetricType == MetricType.Histogram ? series.GetHistogramSum() : null);
    }

    public static Instrument Parse(string line) => JsonSerializer.Deserialize<Instrument>(line)!;

    public string ToLine() => JsonSerializer.Serialize(this);
}
