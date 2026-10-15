using System.Text.RegularExpressions;

namespace MSTest.Reporting.Assertions;

/// <summary>
///     What a run of <see cref="FailingAssertionTests" />, <see cref="CompetingComparisonTests" /> and
///     <see cref="SingleComparisonTests" /> publishes about each failure. MSTest adds an
///     <c>AssertionFailureProperty</c> with the expected and the actual text to the node of a failed test whose
///     failures contain exactly one comparison. <see cref="AssertionFailureReporter" /> reads the property, and the
///     terminal prints it as an <c>Expected</c> and an <c>Actual</c> block below the failure message.
/// </summary>
[TestClass]
public sealed partial class FailingAssertionRunTests
{
    private const int AtLeastOneTestFailed = 2;

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheFailingAssertions(TestContext testContext)
    {
        _run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter",
            $"ClassName={typeof(FailingAssertionTests).FullName}|ClassName={typeof(CompetingComparisonTests).FullName}|ClassName={typeof(SingleComparisonTests).FullName}");
    }

    [ClassCleanup]
    public static void DeleteTheResults()
    {
        _run.Dispose();
    }

    [TestMethod]
    public void Comparison_reports_the_text_of_both_values()
    {
        Assert.Contains($"{nameof(FailingAssertionTests.Compares_two_numbers)} | expected: 15 | actual: 12.5",
            _run.Output);
        Assert.Contains(
            $"{nameof(FailingAssertionTests.Compares_two_strings)} | expected: \"plum\" | actual: \"apple\"",
            _run.Output);
    }

    [TestMethod]
    public void Search_in_a_collection_reports_no_actual_value()
    {
        Assert.Contains(
            $"{nameof(FailingAssertionTests.Looks_for_an_item)} | expected: \"plum\" | actual: {AssertionFailureReporter.NoValue}",
            _run.Output);
    }

    [TestMethod]
    public void Search_in_a_text_reports_the_text_as_actual_value()
    {
        Assert.Contains(
            $"{nameof(FailingAssertionTests.Looks_for_a_part_of_a_text)} | expected: \"plum\" | actual: \"apple pie\"",
            _run.Output);
    }

    [TestMethod]
    public void Failure_without_a_comparison_reports_no_values_where_a_comparison_reports_them()
    {
        Assert.Contains($"{nameof(FailingAssertionTests.Fails_without_a_comparison)} | no values", _run.Output);
        Assert.Contains($"{nameof(FailingAssertionTests.Throws_an_exception)} | no values", _run.Output);
        Assert.Contains($"{nameof(FailingAssertionTests.Compares_two_numbers)} | expected: 15 | actual: 12.5",
            _run.Output);
    }

    [TestMethod]
    public void Only_a_test_with_a_single_comparison_reports_its_values()
    {
        Assert.Contains($"{nameof(CompetingComparisonTests.Fails_in_the_test_and_in_the_cleanup)} | no values",
            _run.Output);
        Assert.Contains(
            $"{nameof(SingleComparisonTests.Fails_in_the_test_and_without_values_in_the_cleanup)} | expected: 15 | actual: 12.5",
            _run.Output);
    }

    [TestMethod]
    public void Terminal_prints_the_values_below_the_failure_message()
    {
        Assert.MatchesRegex(@"Assert\.AreEqual\(15m, Total\)\s+Expected\s+15\s+Actual\s+12\.5\s+at ", _run.Output);
    }

    [TestMethod]
    public void Terminal_prints_the_block_only_for_a_failure_with_values()
    {
        Assert.AreEqual(5, ExpectedValueBlocks().Count(_run.Output));
    }

    [TestMethod]
    public void Run_ends_with_the_exit_code_of_a_failed_test()
    {
        Assert.AreEqual(AtLeastOneTestFailed, _run.ExitCode);
    }

    [GeneratedRegex(@"^\s+Expected$", RegexOptions.Multiline)]
    private static partial Regex ExpectedValueBlocks();
}
