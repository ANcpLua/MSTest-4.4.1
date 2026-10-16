using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OtlpSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OpenTelemetry.Proto.Trace.V1;

namespace MSTest.Platform.Changed.Telemetry;

[TestClass]
public sealed class EnvironmentRunTests(TestContext testContext)
{
    [TestMethod]
    public async Task Endpoint_alone_exports_decodable_spans_and_metrics()
    {
        await using var collector = await OtlpCollector.StartAsync(testContext.CancellationToken);
        using var run = await RunTheCheckoutAsync(new Dictionary<string, string>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = collector.Endpoint,
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf"
        });

        Assert.AreEqual(0, run.ExitCode, run.Output);
        var spans = collector.Requests.Where(request => request.Path == "/v1/traces")
            .SelectMany(request => ExportTraceServiceRequest.Parser.ParseFrom(request.Body).ResourceSpans)
            .SelectMany(resource => resource.ScopeSpans)
            .Where(scope => scope.Scope.Name == "Microsoft.Testing.Platform")
            .SelectMany(scope => scope.Spans).ToArray();
        var span = Assert.ContainsSingle(spans.Where(item => item.Name == nameof(CheckoutTests.Totals_the_items)));
        Assert.AreEqual(16, span.TraceId.Length);
        Assert.AreEqual(8, span.SpanId.Length);
        Assert.Contains(value => value != 0, span.TraceId);
        Assert.Contains(value => value != 0, span.SpanId);
        Assert.AreEqual(OtlpSpan.Types.SpanKind.Internal, span.Kind);
        Assert.AreEqual(Status.Types.StatusCode.Ok, span.Status.Code);
        Assert.IsGreaterThan(0UL, span.StartTimeUnixNano);
        Assert.IsGreaterThanOrEqualTo(span.StartTimeUnixNano, span.EndTimeUnixNano);
        Assert.AreEqual("pass", span.Attributes.Single(attribute => attribute.Key == "test.case.result.status").Value.StringValue);
        AssertMetrics(collector);
    }

    [TestMethod]
    public async Task Exporter_none_disables_remote_traces_without_disabling_metrics()
    {
        await using var collector = await OtlpCollector.StartAsync(testContext.CancellationToken);
        using var run = await RunTheCheckoutAsync(new Dictionary<string, string>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = collector.Endpoint,
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf",
            ["OTEL_TRACES_EXPORTER"] = "none"
        });

        Assert.AreEqual(0, run.ExitCode, run.Output);
        Assert.IsEmpty(collector.Requests.Where(request => request.Path == "/v1/traces"));
        Assert.IsTrue(run.HasSpans);
        AssertMetrics(collector);
    }

    private static void AssertMetrics(OtlpCollector collector)
    {
        var metrics = collector.Requests.Where(request => request.Path == "/v1/metrics")
            .SelectMany(request => ExportMetricsServiceRequest.Parser.ParseFrom(request.Body).ResourceMetrics)
            .SelectMany(resource => resource.ScopeMetrics)
            .Where(scope => scope.Scope.Name == "Microsoft.Testing.Platform")
            .SelectMany(scope => scope.Metrics).ToArray();
        var duration = Assert.ContainsSingle(metrics.Where(metric => metric.Name == "test.case.duration"));
        Assert.AreEqual("s", duration.Unit);
        Assert.AreEqual(2UL, duration.Histogram.DataPoints.Aggregate(0UL, (count, point) => count + point.Count));
        Assert.IsTrue(duration.Histogram.DataPoints.All(point => point.Sum >= 0));
    }

    private Task<TestApplicationRun> RunTheCheckoutAsync(Dictionary<string, string> environment) =>
        TestApplicationRun.StartAsync(environment, testContext.CancellationToken, "--filter",
            $"ClassName={typeof(CheckoutTests).FullName}");
}
