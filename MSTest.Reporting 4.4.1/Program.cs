using Microsoft.Testing.Extensions;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Services;
using MSTest.Reporting.Assertions;
using MSTest.Reporting.Spans;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace MSTest.Reporting;

/// <summary>
/// The entry point of the test application, written by hand because the OpenTelemetry provider is registered
/// in code. <c>AddSelfRegisteredExtensions</c> registers MSTest and the extensions of the project, as the
/// generated entry point does. <c>AddTestingPlatformInstrumentation</c> subscribes the provider to the spans
/// of the platform and of MSTest, and the processor hands each finished span to the exporter.
/// </summary>
internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = await TestApplication.CreateBuilderAsync(args);

        builder.AddSelfRegisteredExtensions(args);
        builder.TestHost.AddDataConsumer(static services => new AssertionFailureReporter(services.GetOutputDevice()));
        builder.AddOpenTelemetryProvider(static tracing => tracing
            .AddTestingPlatformInstrumentation()
            .AddProcessor(new SimpleActivityExportProcessor(new SpanFileExporter())));

        using var application = await builder.BuildAsync();

        return await application.RunAsync();
    }
}
