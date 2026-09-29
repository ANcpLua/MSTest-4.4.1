using System.Diagnostics;
using OpenTelemetry;

namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// An OpenTelemetry exporter that appends each finished span as one line to the file named by the environment
/// variable <c>MSTEST_SPANS</c> and drops the spans when the variable is not set. It stands where a project
/// registers the exporter of its observability backend, so that an example can read the spans of a run.
/// </summary>
internal sealed class SpanFileExporter : BaseExporter<Activity>
{
    public const string Variable = "MSTEST_SPANS";

    private readonly string? _file = Environment.GetEnvironmentVariable(Variable);

    public override ExportResult Export(in Batch<Activity> batch)
    {
        if (_file is null)
        {
            return ExportResult.Success;
        }

        var resource = ParentProvider.GetResource().Attributes;
        foreach (var activity in batch)
        {
            File.AppendAllLines(_file, [Span.From(activity, resource).ToLine()]);
        }

        return ExportResult.Success;
    }
}
