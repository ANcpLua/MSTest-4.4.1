using MSTest.Showcase;

namespace MSTest.Isolation.TempDirectory;

[TestClass]
public sealed class RetentionRunTests(TestContext testContext)
{
    [TestMethod]
    public async Task Failed_test_retains_the_file_for_the_dependent_reader()
    {
        var run = await RunnerResult.RunAsync($"ClassName={typeof(FailedTestRetentionShowcase).FullName}",
            testContext.CancellationToken);

        Assert.AreEqual(2, run.ExitCode, run.Output);
        Assert.AreSequenceEqual([typeof(FailedTestRetentionShowcase).FullName], run.Tests.Select(test => test.ClassName).Distinct());
        Assert.AreSequenceEqual(
            [("Writes_a_file_and_fails", "Failed"), ("Finds_the_file_kept", "Passed")],
            run.Tests.Select(test => (test.Name, test.Outcome)), SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    [DataRow(true, 0, "Passed")]
    [DataRow(false, 2, "Failed")]
    public async Task Environment_switch_controls_passed_test_retention(bool retain, int exitCode, string readerOutcome)
    {
        var run = await RunnerResult.RunAsync($"ClassName={typeof(RetainedByEnvironmentShowcase).FullName}",
            testContext.CancellationToken,
            new Dictionary<string, string?> { ["MSTEST_TEST_TEMP_DIRECTORY_RETAIN"] = retain ? "1" : null });

        Assert.AreEqual(exitCode, run.ExitCode, run.Output);
        Assert.AreSequenceEqual([typeof(RetainedByEnvironmentShowcase).FullName], run.Tests.Select(test => test.ClassName).Distinct());
        Assert.AreSequenceEqual(
            [("Writes_a_file_and_passes", "Passed"), ("Finds_the_file_kept", readerOutcome)],
            run.Tests.Select(test => (test.Name, test.Outcome)), SequenceOrder.InAnyOrder);
    }
}
