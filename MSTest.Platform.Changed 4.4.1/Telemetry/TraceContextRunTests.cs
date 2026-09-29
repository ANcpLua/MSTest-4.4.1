namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// A run that is started with the W3C trace context of its caller in <c>TRACEPARENT</c> and
/// <c>TRACESTATE</c> belongs to the trace of the caller: every span has the trace identifier and the state
/// of the caller, and the span of the run, <c>TestHost</c>, is a child of the span of the caller. Without
/// the variables the span of the run has no parent.
/// </summary>
[TestClass]
public sealed class TraceContextRunTests(TestContext testContext)
{
    private const string Trace = "0af7651916cd43dd8448eb211c80319c";
    private const string Caller = "b7ad6b7169203331";
    private const string State = "shop=checkout";

    [TestMethod]
    public async Task Run_continues_the_trace_of_its_caller()
    {
        using var run = await RunTheCheckoutAsync(new() { ["TRACEPARENT"] = $"00-{Trace}-{Caller}-01", ["TRACESTATE"] = State });
        var spans = run.ReadSpans();

        Assert.AreSequenceEqual([(Trace, State)], spans.Select(static span => (span.TraceId, span.TraceState)).Distinct());
        Assert.AreEqual(Caller, spans.Single(static span => span.Name == "TestHost").ParentSpanId);
    }

    [TestMethod]
    public async Task Run_without_a_caller_starts_a_trace_of_its_own()
    {
        using var run = await RunTheCheckoutAsync([]);
        var host = run.ReadSpans().Single(static span => span.Name == "TestHost");

        Assert.AreEqual("0000000000000000", host.ParentSpanId);
        Assert.AreNotEqual(Trace, host.TraceId);
        Assert.IsNull(host.TraceState);
    }

    private Task<TestApplicationRun> RunTheCheckoutAsync(Dictionary<string, string> environment) =>
        TestApplicationRun.StartAsync(environment, testContext.CancellationToken, "--filter", $"ClassName={typeof(CheckoutTests).FullName}");
}
