using MSTest.Platform.Terminal;

namespace MSTest.Platform.AzureDevOps;

/// <summary>
/// The two toggles of the Azure DevOps report. <c>--report-azdo-annotations</c> controls the
/// <c>##vso[task.logissue type=error]</c> line that the report writes for each failed test: it is on without
/// the option and <c>off</c> leaves the failures without such a line. <c>--report-azdo-groups on</c> wraps
/// the output of the test assembly between <c>##[group]</c> and <c>##[endgroup]</c>, which <c>off</c> leaves
/// out. The report is active with <c>--report-azdo</c> and the variable <c>TF_BUILD</c> set to <c>true</c>,
/// which this example sets itself, as an agent of Azure Pipelines does. The options need
/// <c>EnableMicrosoftTestingExtensionsAzureDevOpsReport</c> in the project file.
/// </summary>
[TestClass]
public sealed class ReportToggleRunTests(TestContext testContext)
{
    private const string Issue = "##vso[task.logissue type=error";
    private const string Group = "##[group]Tests: MSTest.Platform 4.4.1 (net10.0)";
    private const string EndOfGroup = "##[endgroup]";

    [TestMethod]
    public async Task Each_failed_test_gets_an_issue_without_the_option()
    {
        using var run = await RunTheShipmentsAsync();

        var issues = run.Output.Split('\n').Where(static line => line.StartsWith(Issue)).ToList();

        Assert.HasCount(2, issues);
        Assert.ContainsSingle(issues.Where(static issue => issue.Contains(
            $"]{nameof(ShipmentTests.Ships_an_order_that_costs_more_than_was_paid)} [net10.0]%0AAssertion failed.")));
        Assert.ContainsSingle(issues.Where(static issue => issue.Contains(
            $"]{nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)} [net10.0]%0ATest '{nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)}' timed out after 100ms")));
    }

    [TestMethod]
    public async Task Annotations_that_are_off_leave_the_failures_without_an_issue()
    {
        using var run = await RunTheShipmentsAsync("--report-azdo-annotations", "off");

        Assert.DoesNotContain(Issue, run.Output);
        Assert.Contains($"failed {nameof(ShipmentTests.Ships_an_order_that_costs_more_than_was_paid)}", run.ResultLines);
    }

    [TestMethod]
    public async Task Groups_that_are_on_wrap_the_output_of_the_assembly()
    {
        using var run = await RunTheShipmentsAsync("--report-azdo-groups", "on");
        var lines = run.Output.ReplaceLineEndings("\n").Split('\n');

        var start = Array.IndexOf(lines, Group);
        Assert.IsGreaterThanOrEqualTo(0, start);
        Assert.IsGreaterThan(start, Array.IndexOf(lines, EndOfGroup));
    }

    [TestMethod]
    public async Task Groups_that_are_off_leave_the_output_as_it_is()
    {
        using var run = await RunTheShipmentsAsync("--report-azdo-groups", "off");

        Assert.DoesNotContain("##[group]", run.Output);
        Assert.DoesNotContain(EndOfGroup, run.Output);
    }

    private Task<TestApplicationRun> RunTheShipmentsAsync(params string[] toggles) =>
        TestApplicationRun.StartAsync(
            new Dictionary<string, string> { ["TF_BUILD"] = "true" },
            testContext.CancellationToken,
            ["--filter", $"ClassName={typeof(ShipmentTests).FullName}", "--report-azdo", .. toggles]);
}
