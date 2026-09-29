using System.Diagnostics;

namespace MSTest.Reporting.Spans;

/// <summary>
/// The spans that MSTest records for a run of <see cref="LifecycleTests"/>. A test has the span
/// <c>MSTest.TestMethod</c> with its display name, its class, its outcome and the number of its data rows.
/// An assembly, class or test initialize or cleanup method has a span named after its attribute, such as
/// <c>MSTest.ClassInitialize</c>, with the kind of the fixture, its class and its assembly. All of them are
/// children of the <c>TestFramework</c> span of the platform, which stays the current activity of a test.
/// The project file sets <c>EnableMicrosoftTestingExtensionsOpenTelemetry</c> to <c>true</c>, which references
/// the extension, and <c>GenerateTestingPlatformEntryPoint</c> to <c>false</c>, because <c>Program.cs</c>
/// registers the provider with <c>AddOpenTelemetryProvider</c>.
/// </summary>
[TestClass]
public sealed class LifecycleRunTests
{
    private const string TestMethod = "MSTest.TestMethod";

    // Two tests without data, two data rows and two folded data rows.
    private const int TestRuns = 6;

    // Two tests without data, two data rows and one span for the folded data rows.
    private const int TestSpans = 5;

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

        Assert.AreEqual(typeof(LifecycleTests).FullName, span.Attributes["test.suite.name"]);
        Assert.AreEqual(typeof(LifecycleTests).Assembly.Location, span.Attributes["test.assembly.name"]);
        Assert.AreEqual("pass", span.Attributes["test.case.result.status"]);
        Assert.AreEqual("1", span.Attributes["test.case.data_row.count"]);
        Assert.AreEqual(ActivityStatusCode.Unset, span.Status);
    }

    [TestMethod]
    [DataRow("apple")]
    [DataRow("pear")]
    public void Data_row_has_a_span_with_its_display_name(string item) =>
        Assert.AreEqual("1", SpanOf($"{nameof(LifecycleTests.Holds_the_item)} (\"{item}\")").Attributes["test.case.data_row.count"]);

    [TestMethod]
    public void Folded_data_rows_share_one_span_that_counts_them() =>
        Assert.AreEqual("2", SpanOf(nameof(LifecycleTests.Holds_the_item_as_one_test_case)).Attributes["test.case.data_row.count"]);

    [TestMethod]
    public void Only_a_test_that_runs_has_a_span()
    {
        var tests = _spans.Where(static span => span.Name == TestMethod).Select(static span => span.Attributes["test.case.name"]!).ToList();

        Assert.HasCount(TestSpans, tests);
        Assert.DoesNotContain(nameof(LifecycleTests.Is_ignored), tests);
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
    public void Spans_of_MSTest_are_children_of_the_test_framework_span()
    {
        var framework = Assert.ContainsSingle(_spans.Where(static span => span.Name == "TestFramework"));
        var parents = _spans
            .Where(static span => span.Name.StartsWith("MSTest.", StringComparison.Ordinal))
            .Select(static span => (span.TraceId, span.ParentSpanId))
            .Distinct();

        Assert.AreEqual((framework.TraceId, framework.SpanId), Assert.ContainsSingle(parents));
    }

    [TestMethod]
    public void Test_that_sees_the_test_framework_span_as_current_activity_has_a_span_of_its_own() =>
        Assert.AreEqual("pass", SpanOf(nameof(LifecycleTests.Runs_outside_its_own_span)).Attributes["test.case.result.status"]);

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);

    private static Span SpanOf(string test) =>
        Assert.ContainsSingle(_spans.Where(span => span.Name == TestMethod && span.Attributes["test.case.name"] == test));
}
