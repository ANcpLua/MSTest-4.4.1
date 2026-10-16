using MSTest.Showcase;

namespace MSTest.Filtering.Registration;

[TestClass]
public sealed class FilterRunTests(TestContext testContext)
{
    [TestMethod]
    public async Task Registered_filter_runs_skips_and_drops_the_expected_cases()
    {
        var run = await RunnerResult.RunAsync($"ClassName={typeof(CategoryFilterTests).FullName}",
            testContext.CancellationToken);

        Assert.AreEqual(0, run.ExitCode, run.Output);
        Assert.HasCount(2, run.Tests);
        Assert.AreSequenceEqual([typeof(CategoryFilterTests).FullName], run.Tests.Select(test => test.ClassName).Distinct());
        Assert.AreEqual("Passed", run.Tests.Single(test => test.Name == nameof(CategoryFilterTests.Runs_after_the_filter_returned_Run)).Outcome);
        Assert.AreEqual("NotExecuted", run.Tests.Single(test => test.Name == nameof(CategoryFilterTests.Is_skipped_with_the_reason_of_the_filter)).Outcome);
        Assert.DoesNotContain(test => test.Name == nameof(CategoryFilterTests.Is_dropped_without_a_result), run.Tests);
        Assert.Contains("Nightly tests run in the nightly build", run.Report.ToString());
    }
}
