using MSTest.Showcase;

namespace MSTest.Ordering.Cycles;

[TestClass]
public sealed class CycleRunTests(TestContext testContext)
{
    [TestMethod]
    public async Task Cycles_fail_the_five_cycle_nodes_without_executing_their_bodies()
    {
        var run = await RunnerResult.RunAsync("TestCategory=DependencyCycle", testContext.CancellationToken);

        Assert.AreEqual(2, run.ExitCode, run.Output);
        Assert.HasCount(8, run.Tests);
        Assert.AreSequenceEqual(
            ["ConfiguredCycleShowcase.Lock", "ConfiguredCycleShowcase.Unlock", "CycleShowcase.Lock", "CycleShowcase.Unlock", "SelfReferenceShowcase.Lock"],
            run.Tests.Where(test => test.Outcome == "Failed").Select(test => $"{test.ClassName.Split('.').Last()}.{test.Name}"),
            SequenceOrder.InAnyOrder);
        Assert.HasCount(3, run.Tests.Where(test => test.Outcome == "Passed"));
        Assert.Contains("Dependency cycle detected", run.Output);
    }
}
