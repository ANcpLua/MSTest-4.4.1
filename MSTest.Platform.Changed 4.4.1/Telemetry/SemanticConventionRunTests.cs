using System.Diagnostics;

namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// The span that the platform records for a test follows the semantic conventions of OpenTelemetry. It is
/// named after the test and has the attributes <c>test.case.name</c>, <c>test.suite.name</c>,
/// <c>test.case.result.status</c>, <c>code.function.name</c>, <c>code.file.path</c> and
/// <c>code.line.number</c>. A passed test has the status <c>pass</c>. A failed test has the status
/// <c>fail</c>, the attributes <c>error.type</c> and <c>code.stacktrace</c>, the span status <c>Error</c> and
/// an <c>exception</c> event. An inconclusive test has the status <c>skipped</c>, one of the statuses that
/// the platform adds to the two of the conventions, and its reason in <c>test.case.result.explanation</c>.
/// The span of the run, <c>TestHost</c>, has the exit code. The spans need
/// <c>EnableMicrosoftTestingExtensionsOpenTelemetry</c> and <c>GenerateTestingPlatformEntryPoint</c> set to
/// <c>false</c> in the project file, and the registration in <c>Program.cs</c>.
/// </summary>
[TestClass]
public sealed class SemanticConventionRunTests
{
    private const int AtLeastOneTestFailed = 2;

    private static TestApplicationRun _run = null!;
    private static IReadOnlyList<Span> _spans = null!;

    [ClassInitialize]
    public static async Task RunTheCheckoutAndTheDeclinedCard(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(CheckoutTests).FullName}|ClassName={typeof(DeclinedCardTests).FullName}");
        _spans = _run.ReadSpans();
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Passed_test_has_a_span_with_its_name_its_suite_and_its_status()
    {
        var span = SpanOf(nameof(CheckoutTests.Totals_the_items));

        Assert.AreEqual(nameof(CheckoutTests.Totals_the_items), span.Attributes["test.case.name"]);
        Assert.AreEqual(nameof(CheckoutTests), span.Attributes["test.suite.name"]);
        Assert.AreEqual("pass", span.Attributes["test.case.result.status"]);
        Assert.AreEqual(ActivityStatusCode.Ok, span.Status);
        Assert.IsEmpty(span.Events);
    }

    [TestMethod]
    public void Span_has_the_place_of_the_test_in_the_code()
    {
        var span = SpanOf(nameof(CheckoutTests.Totals_the_items));

        Assert.AreEqual($"{typeof(CheckoutTests).FullName}.{nameof(CheckoutTests.Totals_the_items)}", span.Attributes["code.function.name"]);
        Assert.EndsWith($"{nameof(CheckoutTests)}.cs", span.Attributes["code.file.path"]);
        Assert.IsGreaterThan(0, int.Parse(span.Attributes["code.line.number"]!));
    }

    [TestMethod]
    public void Failed_test_has_a_span_with_the_error_and_the_exception()
    {
        var span = SpanOf(nameof(DeclinedCardTests.Charges_the_card));
        var exception = Assert.ContainsSingle(span.Events);

        Assert.AreEqual("fail", span.Attributes["test.case.result.status"]);
        Assert.AreEqual(ActivityStatusCode.Error, span.Status);
        Assert.AreEqual("Microsoft.VisualStudio.TestTools.UnitTesting.MSTestTestNodeException", span.Attributes["error.type"]);
        Assert.Contains(nameof(DeclinedCardTests.Charges_the_card), span.Attributes["code.stacktrace"]!);
        Assert.AreEqual("exception", exception.Name);
        Assert.Contains("The card is declined above its limit.", exception.Attributes["exception.message"]!);
    }

    [TestMethod]
    public void Inconclusive_test_has_a_span_with_the_status_skipped_and_the_reason()
    {
        var span = SpanOf(nameof(DeclinedCardTests.Waits_for_the_bank));

        Assert.AreEqual("skipped", span.Attributes["test.case.result.status"]);
        Assert.AreEqual("Assert.Inconclusive. The bank has not answered.", span.Attributes["test.case.result.explanation"]);
        Assert.AreEqual(ActivityStatusCode.Unset, span.Status);
    }

    [TestMethod]
    public void Run_has_a_span_with_the_exit_code()
    {
        var span = SpanOf("TestHost");

        Assert.AreEqual("2", span.Attributes["test.run.exit_code"]);
        Assert.AreEqual("1", span.Attributes["test.run.failed"]);
        Assert.AreEqual("1", span.Attributes["test.run.skipped"]);
        Assert.AreEqual(ActivityStatusCode.Error, span.Status);
        Assert.AreEqual(AtLeastOneTestFailed, _run.ExitCode);
    }

    private static Span SpanOf(string name) => Assert.ContainsSingle(_spans.Where(span => span.Name == name));
}
