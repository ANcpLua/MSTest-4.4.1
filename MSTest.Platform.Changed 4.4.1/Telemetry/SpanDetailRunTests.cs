namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
///     Three variables set how much the platform puts on the span of a test. The output that a test writes is in
///     <c>test.output.stdout</c> unless <c>TESTINGPLATFORM_OTEL_CAPTURE_TEST_OUTPUT</c> is <c>0</c>. The legacy
///     attributes, such as <c>test.name</c> and <c>test.result</c>, stand next to those of the conventions
///     unless <c>TESTINGPLATFORM_OTEL_EMIT_LEGACY_ATTRIBUTES</c> is <c>0</c>.
///     <c>TESTINGPLATFORM_OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT</c> is the number of characters that the captured
///     output, the result explanation and the exception message and stack trace keep before an ellipsis. The
///     other attributes, such as the name of the test, are exported whole.
/// </summary>
[TestClass]
public sealed class SpanDetailRunTests(TestContext testContext)
{
    private const string Receipt = nameof(CheckoutTests.Prints_the_receipt);

    [TestMethod]
    public async Task Span_has_the_output_and_the_legacy_attributes_by_default()
    {
        var span = await SpanOfTheReceiptAsync([]);

        Assert.AreEqual($"Receipt of two items{Environment.NewLine}", span.Attributes["test.output.stdout"]);
        Assert.AreEqual(Receipt, span.Attributes["test.name"]);
        Assert.AreEqual("passed", span.Attributes["test.result"]);
    }

    [TestMethod]
    public async Task Span_has_no_output_when_the_capture_is_off()
    {
        var span = await SpanOfTheReceiptAsync(new Dictionary<string, string>
            { ["TESTINGPLATFORM_OTEL_CAPTURE_TEST_OUTPUT"] = "0" });

        Assert.DoesNotContain("test.output.stdout", span.Attributes.Keys);
        Assert.AreEqual(Receipt, span.Attributes["test.case.name"]);
    }

    [TestMethod]
    public async Task Span_has_the_attributes_of_the_conventions_only_when_the_legacy_attributes_are_off()
    {
        var span = await SpanOfTheReceiptAsync(new Dictionary<string, string>
            { ["TESTINGPLATFORM_OTEL_EMIT_LEGACY_ATTRIBUTES"] = "0" });

        Assert.DoesNotContain("test.name", span.Attributes.Keys);
        Assert.DoesNotContain("test.result", span.Attributes.Keys);
        Assert.AreEqual(Receipt, span.Attributes["test.case.name"]);
        Assert.AreEqual("pass", span.Attributes["test.case.result.status"]);
    }

    [TestMethod]
    public async Task Span_shortens_the_output_to_the_limit_and_keeps_the_name_whole()
    {
        var span = await SpanOfTheReceiptAsync(new Dictionary<string, string>
            { ["TESTINGPLATFORM_OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT"] = "7" });

        Assert.AreEqual("Receipt…", span.Attributes["test.output.stdout"]);
        Assert.AreEqual(Receipt, span.Attributes["test.case.name"]);
    }

    private async Task<Span> SpanOfTheReceiptAsync(Dictionary<string, string> environment)
    {
        using var run = await TestApplicationRun.StartAsync(
            environment,
            testContext.CancellationToken,
            "--filter", $"FullyQualifiedName={typeof(CheckoutTests).FullName}.{Receipt}");

        return Assert.ContainsSingle(run.ReadSpans().Where(static span => span.Name == Receipt));
    }
}