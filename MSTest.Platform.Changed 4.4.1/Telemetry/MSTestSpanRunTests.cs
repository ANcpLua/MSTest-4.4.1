namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// The spans that MSTest adds to those of the platform. A test method has the span
/// <c>MSTest.TestMethod</c> with the name of the test, the full name of its class and its status. A call of
/// a fixture has a span named after the attribute of the fixture, such as <c>MSTest.ClassInitialize</c>,
/// with the kind of the fixture and the full name of its class. All of them are children of the
/// <c>TestFramework</c> span of the platform, which names MSTest and its version.
/// </summary>
[TestClass]
public sealed class MSTestSpanRunTests
{
    private const string TestMethod = "MSTest.TestMethod";

    private static TestApplicationRun _run = null!;
    private static IReadOnlyList<Span> _spans = null!;

    [ClassInitialize]
    public static async Task RunTheCheckout(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(testContext.CancellationToken, "--filter", $"ClassName={typeof(CheckoutTests).FullName}");
        _spans = _run.ReadSpans();
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    [DataRow(nameof(CheckoutTests.Totals_the_items))]
    [DataRow(nameof(CheckoutTests.Prints_the_receipt))]
    public void Test_method_has_a_span_with_its_name_its_class_and_its_status(string test)
    {
        var span = Assert.ContainsSingle(_spans.Where(span => span.Name == TestMethod && span.Attributes["test.case.name"] == test));

        Assert.AreEqual(typeof(CheckoutTests).FullName, span.Attributes["test.suite.name"]);
        Assert.AreEqual("pass", span.Attributes["test.case.result.status"]);
    }

    [TestMethod]
    [DataRow("MSTest.ClassInitialize", "class_initialize", 1)]
    [DataRow("MSTest.TestInitialize", "test_initialize", 2)]
    public void Fixture_has_a_span_for_each_of_its_calls(string name, string kind, int calls)
    {
        var spans = _spans.Where(span => span.Name == name).ToArray();

        Assert.HasCount(calls, spans);
        Assert.AreSequenceEqual([kind], spans.Select(static span => span.Attributes["test.fixture.kind"]).Distinct());
        Assert.AreSequenceEqual([typeof(CheckoutTests).FullName], spans.Select(static span => span.Attributes["test.suite.name"]).Distinct());
    }

    [TestMethod]
    public void Spans_of_MSTest_are_children_of_the_span_of_the_test_framework()
    {
        var framework = Assert.ContainsSingle(_spans.Where(static span => span.Name == "TestFramework"));
        var parents = _spans
            .Where(static span => span.Name.StartsWith("MSTest.", StringComparison.Ordinal))
            .Select(static span => (span.TraceId, span.ParentSpanId))
            .Distinct();

        Assert.AreEqual((framework.TraceId, framework.SpanId), Assert.ContainsSingle(parents));
        Assert.AreEqual("MSTest", framework.Attributes["test.framework.name"]);
        Assert.AreEqual("4.4.1", framework.Attributes["test.framework.version"]);
    }

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);
}
