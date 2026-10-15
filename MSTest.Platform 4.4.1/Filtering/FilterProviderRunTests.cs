namespace MSTest.Platform.Filtering;

/// <summary>
///     Runs of <see cref="RouteTests" /> with the two providers of <c>Program.cs</c>: the list of the changed tests
///     constrains discovery and runs, the list of the nightly tests constrains runs only. A test runs when every
///     provider and the <c>--filter</c> of MSTest accept it, and a run that is left without a test ends with
///     exit code 8.
/// </summary>
[TestClass]
public sealed class FilterProviderRunTests(TestContext testContext)
{
    private const string Road = $"passed {nameof(RouteTests.Plans_a_route_by_road)}";
    private const string Rail = $"passed {nameof(RouteTests.Plans_a_route_by_rail)}";
    private const int ZeroTests = 8;

    [TestMethod]
    public async Task Provider_limits_the_run_to_the_tests_of_its_list()
    {
        using var run = await RunAsync(new Dictionary<string, string>
            { [TestListFilter.ChangedTests] = $"{RouteTests.ByRoad},{RouteTests.ByRail}" });

        Assert.AreSequenceEqual([Road, Rail], run.ResultLines, SequenceOrder.InAnyOrder);
        Assert.AreEqual(0, run.ExitCode);
    }

    [TestMethod]
    public async Task Two_providers_limit_the_run_to_the_tests_of_both_lists()
    {
        using var run = await RunAsync(new Dictionary<string, string>
        {
            [TestListFilter.ChangedTests] = $"{RouteTests.ByRoad},{RouteTests.ByRail}",
            [TestListFilter.NightlyTests] = $"{RouteTests.ByRail},{RouteTests.BySea}"
        });

        Assert.AreSequenceEqual([Rail], run.ResultLines);
    }

    [TestMethod]
    public async Task Filter_of_MSTest_applies_together_with_the_provider()
    {
        using var run = await RunAsync(
            new Dictionary<string, string>
                { [TestListFilter.ChangedTests] = $"{RouteTests.ByRoad},{RouteTests.ByRail}" },
            "--filter",
            $"Name={nameof(RouteTests.Plans_a_route_by_road)}|Name={nameof(RouteTests.Plans_a_route_by_sea)}");

        Assert.AreSequenceEqual([Road], run.ResultLines);
    }

    [TestMethod]
    public async Task Lists_without_a_common_test_leave_nothing_to_run()
    {
        using var run = await RunAsync(new Dictionary<string, string>
        {
            [TestListFilter.ChangedTests] = RouteTests.ByRoad,
            [TestListFilter.NightlyTests] = RouteTests.BySea
        });

        Assert.IsEmpty(run.ResultLines);
        Assert.AreEqual(ZeroTests, run.ExitCode);
    }

    [TestMethod]
    public async Task Discovery_applies_the_provider_that_constrains_discovery_only()
    {
        using var run = await RunAsync(
            new Dictionary<string, string>
            {
                [TestListFilter.ChangedTests] = $"{RouteTests.ByRoad},{RouteTests.ByRail}",
                [TestListFilter.NightlyTests] = RouteTests.BySea
            },
            "--list-tests");

        Assert.Contains("Test discovery summary: found 2 test(s)", run.Output);
        Assert.Contains(nameof(RouteTests.Plans_a_route_by_road), run.Output);
        Assert.Contains(nameof(RouteTests.Plans_a_route_by_rail), run.Output);
    }

    private Task<TestApplicationRun> RunAsync(Dictionary<string, string> lists, params string[] options)
    {
        return TestApplicationRun.StartAsync(lists, testContext.CancellationToken,
            ["--output", "detailed", .. options]);
    }
}