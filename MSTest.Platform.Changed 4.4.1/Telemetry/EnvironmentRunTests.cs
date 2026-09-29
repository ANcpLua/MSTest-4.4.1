namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// <c>AddOpenTelemetryProviderFromEnvironment</c> selects the exporter from the <c>OTEL_*</c> variables of
/// the run. <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> alone adds the OTLP exporter for spans and instruments, and
/// <c>OTEL_TRACES_EXPORTER</c> and <c>OTEL_METRICS_EXPORTER</c> select <c>otlp</c> or <c>none</c> for each
/// of them. The exporter belongs to the OpenTelemetry SDK, whose variable
/// <c>OTEL_EXPORTER_OTLP_PROTOCOL</c> selects the protocol that the collector of this example understands.
/// </summary>
[TestClass]
public sealed class EnvironmentRunTests(TestContext testContext)
{
    private const string Traces = "/v1/traces";
    private const string Metrics = "/v1/metrics";

    [TestMethod]
    public async Task Endpoint_alone_exports_the_spans_and_the_instruments()
    {
        await using var collector = new OtlpCollector();

        using var run = await RunTheCheckoutAsync(new()
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = collector.Endpoint,
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf",
        });

        Assert.Contains(nameof(CheckoutTests.Totals_the_items), BodiesOf(collector, Traces));
        Assert.Contains("test.case.duration", BodiesOf(collector, Metrics));
    }

    [TestMethod]
    public async Task Exporter_none_keeps_the_spans_from_the_endpoint()
    {
        await using var collector = new OtlpCollector();

        using var run = await RunTheCheckoutAsync(new()
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = collector.Endpoint,
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf",
            ["OTEL_TRACES_EXPORTER"] = "none",
        });

        Assert.IsEmpty(BodiesOf(collector, Traces));
        Assert.Contains("test.case.duration", BodiesOf(collector, Metrics));
        Assert.IsTrue(run.HasSpans);
    }

    private static string BodiesOf(OtlpCollector collector, string path) =>
        string.Concat(collector.Requests.Where(request => request.Path == path).Select(static request => request.Body));

    private Task<TestApplicationRun> RunTheCheckoutAsync(Dictionary<string, string> environment) =>
        TestApplicationRun.StartAsync(environment, testContext.CancellationToken, "--filter", $"ClassName={typeof(CheckoutTests).FullName}");
}
