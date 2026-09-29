namespace MSTest.Platform.Assertions;

/// <summary>
/// What a run of <see cref="PriceTests"/> reports about each failure. The node of a failed comparison carries
/// the texts of both values, the node of a failed search the expected value only, and a failure that compares
/// nothing carries no values. <see cref="AssertionValueReporter"/> prints what it receives, and the terminal
/// prints the values as an <c>Expected</c> and an <c>Actual</c> block below the message of the failure.
/// </summary>
[TestClass]
public sealed class AssertionFailureRunTests
{
    private const int AtLeastOneTestFailed = 2;

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunThePriceTests(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(testContext.CancellationToken, "--filter", $"ClassName={typeof(PriceTests).FullName}");

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Comparison_carries_the_texts_of_both_values() =>
        Assert.Contains($"Values of {nameof(PriceTests.Adds_the_tax_to_the_net_price)}: expected 119, actual 107.00", _run.Output);

    [TestMethod]
    public void Search_carries_the_expected_value_only() =>
        Assert.Contains($"Values of {nameof(PriceTests.Accepts_the_currency)}: expected \"CHF\", actual {AssertionValueReporter.None}", _run.Output);

    [TestMethod]
    public void Failure_that_compares_nothing_carries_no_values()
    {
        Assert.Contains($"Values of {nameof(PriceTests.Rounds_the_price)}: no values", _run.Output);
        Assert.Contains($"Values of {nameof(PriceTests.Converts_the_price)}: no values", _run.Output);
    }

    [TestMethod]
    public void Terminal_prints_the_values_below_the_message_of_the_failure() =>
        Assert.MatchesRegex(@"Assert\.AreEqual\(119m, Net \* 1\.07m\)\s+Expected\s+119\s+Actual\s+107\.00\s+at ", _run.Output);

    [TestMethod]
    public void Run_ends_with_the_exit_code_of_a_failed_test() => Assert.AreEqual(AtLeastOneTestFailed, _run.ExitCode);
}
