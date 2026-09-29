using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// An OpenTelemetry exporter that appends each series of each instrument as one line to the file named by the
/// environment variable <c>MSTEST_INSTRUMENTS</c> and drops them when the variable is not set. The reader that
/// <c>Program.cs</c> registers hands the instruments over when the provider shuts down at the end of the run.
/// </summary>
internal sealed class InstrumentFileExporter : BaseExporter<Metric>
{
    public const string Variable = "MSTEST_INSTRUMENTS";

    private readonly string? _file = Environment.GetEnvironmentVariable(Variable);

    public override ExportResult Export(in Batch<Metric> batch)
    {
        if (_file is null)
        {
            return ExportResult.Success;
        }

        foreach (var metric in batch)
        {
            foreach (ref readonly var series in metric.GetMetricPoints())
            {
                File.AppendAllLines(_file, [Instrument.From(metric, series).ToLine()]);
            }
        }

        return ExportResult.Success;
    }
}
