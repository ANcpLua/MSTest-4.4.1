using Microsoft.Testing.Extensions;
using Microsoft.Testing.Platform.Builder;
using MSTest.Platform.Changed.Telemetry;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MSTest.Platform.Changed;

/// <summary>
/// The entry point of the test application, written by hand because the OpenTelemetry provider is registered
/// in code. <c>AddSelfRegisteredExtensions</c> registers MSTest and the extensions of the project, as the
/// generated entry point does. <c>AddOpenTelemetryProviderFromEnvironment</c> subscribes the provider to the
/// spans and metrics of the platform, adds the resource of the run and reads the exporter from the
/// <c>OTEL_*</c> variables. The two callbacks run after that and add the exporters of the examples.
/// </summary>
internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = await TestApplication.CreateBuilderAsync(args);

        builder.AddSelfRegisteredExtensions(args);
        builder.AddOpenTelemetryProviderFromEnvironment(
            static tracing => tracing.AddProcessor(new SimpleActivityExportProcessor(new SpanFileExporter())),
            static metrics => metrics.AddReader(new BaseExportingMetricReader(new InstrumentFileExporter())));

        using var application = await builder.BuildAsync();

        return await application.RunAsync();
    }
}
