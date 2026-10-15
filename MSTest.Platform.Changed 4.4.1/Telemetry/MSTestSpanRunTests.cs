namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// The spans that MSTest adds to those of the platform. A test method has one canonical span
/// named after the test, with its class, namespace and status. A call of
/// a fixture has a span named after the attribute of the fixture, such as <c>MSTest.ClassInitialize</c>,
/// with the kind of the fixture and the full name of its class. Class fixtures and tests are children of
/// <c>TestFramework</c>; test fixtures are children of their test. The framework span names MSTest and its version.
/// </summary>
[TestClass]
public sealed class MSTestSpanRunTests
{
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
        var span = Assert.ContainsSingle(_spans.Where(span => span.Name == test && span.Attributes.ContainsKey("test.case.id")));

        Assert.AreEqual(test, span.Attributes["test.case.name"]);
        Assert.AreEqual(nameof(CheckoutTests), span.Attributes["test.suite.name"]);
        Assert.AreEqual(typeof(CheckoutTests).Namespace, span.Attributes["test.namespace"]);
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
    public void Test_spans_belong_to_the_framework_and_test_fixtures_belong_to_their_test()
    {
        var framework = Assert.ContainsSingle(_spans.Where(static span => span.Name == "TestFramework"));
        var tests = _spans.Where(static span => span.Attributes.ContainsKey("test.case.id")).ToArray();
        Assert.HasCount(2, tests);
        foreach (var test in tests)
        {
            Assert.AreEqual((framework.TraceId, framework.SpanId), (test.TraceId, test.ParentSpanId));
            var fixture = Assert.ContainsSingle(_spans.Where(span => span.Name == "MSTest.TestInitialize" && span.ParentSpanId == test.SpanId));
            Assert.AreEqual(test.TraceId, fixture.TraceId);
        }

        var classFixture = Assert.ContainsSingle(_spans.Where(static span => span.Name == "MSTest.ClassInitialize"));
        Assert.AreEqual((framework.TraceId, framework.SpanId), (classFixture.TraceId, classFixture.ParentSpanId));
        Assert.IsEmpty(_spans.Where(static span => span.Name == "MSTest.TestMethod"));
        Assert.AreEqual("MSTest", framework.Attributes["test.framework.name"]);
        Assert.AreEqual("4.5.1", framework.Attributes["test.framework.version"]);
    }

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);
}
