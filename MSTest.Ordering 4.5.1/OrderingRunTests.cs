using MSTest.Showcase;

namespace MSTest.Ordering;

[TestClass]
public sealed class OrderingRunTests(TestContext testContext)
{
    [TestMethod]
    public async Task Native_dependencies_run_all_scenarios_and_preserve_expected_failure_results()
    {
        var run = await RunnerResult.RunAsync("TestCategory=OrderingScenario", testContext.CancellationToken);

        Assert.AreEqual(2, run.ExitCode, run.Output);
        Assert.HasCount(65, run.Tests);
        Assert.IsTrue(run.Tests.All(test => test.ClassName.StartsWith("MSTest.Ordering.", StringComparison.Ordinal)));
        Assert.AreSequenceEqual(["Charge", "Deploy"],
            run.Tests.Where(test => test.Outcome == "Failed").Select(test => test.Name), SequenceOrder.InAnyOrder);
        Assert.AreSequenceEqual(["Refund", "Invoice", "Ship", "Announce", "Verify"],
            run.Tests.Where(test => test.Outcome == "NotExecuted").Select(test => test.Name), SequenceOrder.InAnyOrder);
        Assert.HasCount(58, run.Tests.Where(test => test.Outcome == "Passed"));
    }
}
