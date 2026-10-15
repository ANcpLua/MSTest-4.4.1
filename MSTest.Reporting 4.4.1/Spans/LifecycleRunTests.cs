using System.Diagnostics;

namespace MSTest.Reporting.Spans;

/// <summary>
/// The spans that MSTest records for a run of <see cref="LifecycleTests"/>. A test has the span
/// named after its display name, with its class, namespace and final outcome.
/// An assembly, class or test initialize or cleanup method has a span named after its attribute, such as
/// <c>MSTest.ClassInitialize</c>, with the kind of the fixture, its class and its assembly. All of them are
/// children of <c>TestFramework</c> for assembly and class fixtures, or of the canonical test span for test
/// fixtures. The canonical test span is the current activity during the test.
/// The project file sets <c>EnableMicrosoftTestingExtensionsOpenTelemetry</c> to <c>true</c>, which references
/// the extension, and <c>GenerateTestingPlatformEntryPoint</c> to <c>false</c>, because <c>Program.cs</c>
/// registers the provider with <c>AddOpenTelemetryProvider</c>.
/// </summary>
[TestClass]
public sealed class LifecycleRunTests
{
    // Two tests without data, two data rows and two folded data rows.
    private const int TestRuns = 6;

    // Two tests without data, two data rows, one folded test and one ignored test result.
    private const int TestSpans = 6;

    private static TestApplicationRun _run = null!;
    private static IReadOnlyList<Span> _spans = null!;

    [ClassInitialize]
    public static async Task RunTheLifecycleTests(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(LifecycleTests).FullName}");
        _spans = _run.ReadSpans();
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Test_method_has_a_span_with_its_name_its_class_and_its_outcome()
    {
        var span = SpanOf(nameof(LifecycleTests.Holds_the_stock_of_the_open_shop));

        Assert.AreEqual(nameof(LifecycleTests), span.Attributes["test.suite.name"]);
        Assert.AreEqual(typeof(LifecycleTests).Namespace, span.Attributes["test.namespace"]);
        Assert.AreEqual($"{typeof(LifecycleTests).FullName}.{nameof(LifecycleTests.Holds_the_stock_of_the_open_shop)}", span.Attributes["code.function.name"]);
        Assert.AreEqual("pass", span.Attributes["test.case.result.status"]);
        Assert.AreEqual(ActivityStatusCode.Ok, span.Status);
    }

    [TestMethod]
    [DataRow("apple")]
    [DataRow("pear")]
    public void Data_row_has_a_span_with_its_display_name(string item)
    {
        var name = $"{nameof(LifecycleTests.Holds_the_item)} (\"{item}\")";
        var span = SpanOf(name);
        Assert.AreEqual(name, span.Name);
        Assert.AreEqual("pass", span.Attributes["test.case.result.status"]);
    }

    [TestMethod]
    public void Folded_data_rows_share_one_span_with_both_fixture_calls()
    {
        var span = SpanOf(nameof(LifecycleTests.Holds_the_item_as_one_test_case));
        Assert.AreEqual("pass", span.Attributes["test.case.result.status"]);
        Assert.HasCount(2, _spans.Where(fixture => fixture.Name == "MSTest.TestInitialize" && fixture.ParentSpanId == span.SpanId));
        Assert.HasCount(2, _spans.Where(fixture => fixture.Name == "MSTest.TestCleanup" && fixture.ParentSpanId == span.SpanId));
    }

    [TestMethod]
    public void Every_test_result_has_one_span_including_the_ignored_test()
    {
        var tests = _spans.Where(static span => span.Attributes.ContainsKey("test.case.id")).ToArray();

        Assert.HasCount(TestSpans, tests);
        var ignored = SpanOf(nameof(LifecycleTests.Is_ignored));
        Assert.AreEqual("skipped", ignored.Attributes["test.case.result.status"]);
        Assert.IsEmpty(_spans.Where(span => span.ParentSpanId == ignored.SpanId));
    }

    [TestMethod]
    [DataRow("MSTest.AssemblyInitialize", "assembly_initialize")]
    [DataRow("MSTest.AssemblyCleanup", "assembly_cleanup")]
    [DataRow("MSTest.ClassInitialize", "class_initialize")]
    [DataRow("MSTest.ClassCleanup", "class_cleanup")]
    public void Assembly_or_class_fixture_has_one_span(string name, string kind)
    {
        var span = Assert.ContainsSingle(_spans.Where(span => span.Name == name));

        Assert.AreEqual(kind, span.Attributes["test.fixture.kind"]);
        Assert.AreEqual(typeof(LifecycleTests).FullName, span.Attributes["test.suite.name"]);
        Assert.AreEqual(typeof(LifecycleTests).Assembly.GetName().Name, span.Attributes["test.assembly.name"]);
    }

    [TestMethod]
    [DataRow("MSTest.TestInitialize", "test_initialize")]
    [DataRow("MSTest.TestCleanup", "test_cleanup")]
    public void Test_fixture_has_a_span_for_each_run_of_a_test(string name, string kind)
    {
        var kinds = _spans.Where(span => span.Name == name).Select(static span => span.Attributes["test.fixture.kind"]);

        Assert.AreSequenceEqual(Enumerable.Repeat(kind, TestRuns), kinds);
    }

    [TestMethod]
    public void Assembly_and_class_fixtures_belong_to_the_framework_and_test_fixtures_to_their_test()
    {
        var framework = Assert.ContainsSingle(_spans.Where(static span => span.Name == "TestFramework"));
        var tests = _spans.Where(static span => span.Attributes.ContainsKey("test.case.id")).ToArray();
        foreach (var test in tests)
        {
            Assert.AreEqual((framework.TraceId, framework.SpanId), (test.TraceId, test.ParentSpanId));
        }

        foreach (var fixture in _spans.Where(static span => span.Attributes.ContainsKey("test.fixture.kind")))
        {
            var kind = fixture.Attributes["test.fixture.kind"]!;
            if (kind.StartsWith("test_", StringComparison.Ordinal))
            {
                var test = Assert.ContainsSingle(tests.Where(test => test.SpanId == fixture.ParentSpanId));
                Assert.AreEqual(test.TraceId, fixture.TraceId);
            }
            else
            {
                Assert.AreEqual((framework.TraceId, framework.SpanId), (fixture.TraceId, fixture.ParentSpanId));
            }
        }

        Assert.IsEmpty(_spans.Where(static span => span.Name == "MSTest.TestMethod"));
    }

    [TestMethod]
    public void Test_sees_its_own_span_as_the_current_activity() =>
        Assert.AreEqual("pass", SpanOf(nameof(LifecycleTests.Runs_inside_its_own_span)).Attributes["test.case.result.status"]);

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);

    private static Span SpanOf(string test) =>
        Assert.ContainsSingle(_spans.Where(span => span.Name == test && span.Attributes.TryGetValue("test.case.name", out var name) && name == test));
}
