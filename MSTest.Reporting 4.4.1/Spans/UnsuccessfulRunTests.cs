using System.Diagnostics;

namespace MSTest.Reporting.Spans;

/// <summary>
/// The spans that MSTest records for a run of <see cref="UnsuccessfulTests"/>, <see cref="SlowFixtureTests"/>,
/// <see cref="ThrowingFixtureTests"/>, <see cref="ThrowingClassInitializeTests"/> and
/// <see cref="ThrowingClassCleanupTests"/>. The span of a failed test has the outcome <c>fail</c>, the status
/// <c>Error</c> and an <c>exception</c> event with the failure message. The span of an inconclusive test has
/// the outcome <c>skipped</c> and the status <c>Unset</c>. The span of a test initialize method records a
/// timeout in the same way as a failed test. The span of a test initialize, class initialize or class cleanup
/// method does not record an exception that the method throws. For a test initialize method that failure is
/// on the canonical span of the test. A class initialize failure produces a failed test result span even
/// though the body does not run; a class cleanup failure changes the final result span to failed.
/// </summary>
[TestClass]
public sealed class UnsuccessfulRunTests
{
    private const int AtLeastOneTestFailed = 2;

    private static TestApplicationRun _run = null!;
    private static IReadOnlyList<Span> _spans = null!;

    [ClassInitialize]
    public static async Task RunTheUnsuccessfulTests(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter",
            $"ClassName={typeof(UnsuccessfulTests).FullName}|ClassName={typeof(SlowFixtureTests).FullName}|ClassName={typeof(ThrowingFixtureTests).FullName}|ClassName={typeof(ThrowingClassInitializeTests).FullName}|ClassName={typeof(ThrowingClassCleanupTests).FullName}");
        _spans = _run.ReadSpans();
    }

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Failed_test_has_a_span_with_the_status_error_and_the_exception()
    {
        var span = SpanOf(nameof(UnsuccessfulTests.Finds_a_missing_item));
        var exception = Assert.ContainsSingle(span.Events);

        Assert.AreEqual("fail", span.Attributes["test.case.result.status"]);
        Assert.AreEqual(ActivityStatusCode.Error, span.Status);
        Assert.AreEqual("exception", exception.Name);
        Assert.StartsWith("Assertion failed. Expected collection to contain the specified element.", exception.Attributes["exception.message"]);
    }

    [TestMethod]
    public void Inconclusive_test_has_a_span_with_the_outcome_skipped()
    {
        var span = SpanOf(nameof(UnsuccessfulTests.Waits_for_a_delivery));

        Assert.AreEqual("skipped", span.Attributes["test.case.result.status"]);
        Assert.AreEqual(ActivityStatusCode.Unset, span.Status);
        Assert.IsEmpty(span.Events);
    }

    [TestMethod]
    public void Test_fixture_that_times_out_has_a_span_with_the_status_error_and_the_exception()
    {
        var span = FixtureSpanOf("MSTest.TestInitialize", typeof(SlowFixtureTests));
        var exception = Assert.ContainsSingle(span.Events);

        Assert.AreEqual(ActivityStatusCode.Error, span.Status);
        Assert.AreEqual("exception", exception.Name);
        Assert.AreEqual(
            $"Test initialize method '{typeof(SlowFixtureTests).FullName}.{nameof(SlowFixtureTests.WaitForTheDelivery)}' timed out after 100ms",
            exception.Attributes["exception.message"]);
    }

    [TestMethod]
    public void Test_fixture_that_throws_leaves_the_exception_to_the_span_of_the_test()
    {
        var fixture = FixtureSpanOf("MSTest.TestInitialize", typeof(ThrowingFixtureTests));
        var test = SpanOf(nameof(ThrowingFixtureTests.Is_not_reached_after_the_exception));

        Assert.AreEqual(ActivityStatusCode.Unset, fixture.Status);
        Assert.IsEmpty(fixture.Events);
        Assert.AreEqual(ActivityStatusCode.Error, test.Status);
        Assert.Contains("The basket is locked.", Assert.ContainsSingle(test.Events).Attributes["exception.message"]!);
    }

    [TestMethod]
    public void Class_initialize_that_throws_leaves_the_exception_on_the_failed_test_span()
    {
        var fixture = FixtureSpanOf("MSTest.ClassInitialize", typeof(ThrowingClassInitializeTests));

        Assert.AreEqual(ActivityStatusCode.Unset, fixture.Status);
        Assert.IsEmpty(fixture.Events);
        var test = SpanOf(nameof(ThrowingClassInitializeTests.Is_not_reached_after_the_class_fixture));
        Assert.AreEqual("fail", test.Attributes["test.case.result.status"]);
        Assert.AreEqual(ActivityStatusCode.Error, test.Status);
        Assert.Contains("The shelf is locked.", Assert.ContainsSingle(test.Events).Attributes["exception.message"]!);
    }

    [TestMethod]
    public void Class_cleanup_that_throws_changes_the_final_test_span_to_failed()
    {
        var fixture = FixtureSpanOf("MSTest.ClassCleanup", typeof(ThrowingClassCleanupTests));
        var test = SpanOf(nameof(ThrowingClassCleanupTests.Holds_an_item_before_the_class_fixture_throws));

        Assert.AreEqual(ActivityStatusCode.Unset, fixture.Status);
        Assert.IsEmpty(fixture.Events);
        Assert.AreEqual("fail", test.Attributes["test.case.result.status"]);
        Assert.AreEqual(ActivityStatusCode.Error, test.Status);
        Assert.Contains("The shelf is stuck.", Assert.ContainsSingle(test.Events).Attributes["exception.message"]!);
    }

    [TestMethod]
    public void Run_ends_with_the_exit_code_of_a_failed_test() => Assert.AreEqual(AtLeastOneTestFailed, _run.ExitCode);

    private static Span SpanOf(string test) =>
        Assert.ContainsSingle(_spans.Where(span => span.Name == test && span.Attributes.TryGetValue("test.case.name", out var name) && name == test));

    private static Span FixtureSpanOf(string name, Type testClass) =>
        Assert.ContainsSingle(_spans.Where(span => span.Name == name && span.Attributes["test.suite.name"] == testClass.FullName));
}
