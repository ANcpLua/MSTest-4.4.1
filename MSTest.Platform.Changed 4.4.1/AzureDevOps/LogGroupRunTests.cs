namespace MSTest.Platform.Changed.AzureDevOps;

/// <summary>
/// The Azure DevOps report wraps the output of a test assembly in a log group only on request:
/// <c>--report-azdo-groups on</c> writes <c>##[group]</c> before the tests and <c>##[endgroup]</c> after
/// them, and without the option, or with <c>off</c>, the output has no group. The report is active with
/// <c>--report-azdo</c> and the variable <c>TF_BUILD</c> set to <c>true</c>, which this example sets itself,
/// as an agent of Azure Pipelines does. The options need
/// <c>EnableMicrosoftTestingExtensionsAzureDevOpsReport</c> in the project file.
/// </summary>
[TestClass]
public sealed class LogGroupRunTests(TestContext testContext)
{
    private const string Group = "##[group]";
    private const string EndOfGroup = "##[endgroup]";

    [TestMethod]
    public async Task Output_has_no_group_by_default()
    {
        using var run = await RunThePackingAsync();

        Assert.DoesNotContain(Group, run.Output);
        Assert.DoesNotContain(EndOfGroup, run.Output);
        Assert.AreEqual(0, run.ExitCode);
    }

    [TestMethod]
    public async Task Output_has_no_group_when_the_groups_are_off()
    {
        using var run = await RunThePackingAsync("--report-azdo-groups", "off");

        Assert.DoesNotContain(Group, run.Output);
        Assert.DoesNotContain(EndOfGroup, run.Output);
    }

    [TestMethod]
    public async Task Output_has_a_group_for_the_assembly_when_the_groups_are_on()
    {
        using var run = await RunThePackingAsync("--report-azdo-groups", "on");
        var lines = run.Output.Split(Environment.NewLine);

        var start = Array.IndexOf(lines, $"{Group}Tests: {typeof(PackingTests).Assembly.GetName().Name} (net10.0)");
        var end = Array.IndexOf(lines, EndOfGroup);

        Assert.IsGreaterThanOrEqualTo(0, start);
        Assert.IsGreaterThan(start, end);
        Assert.AreEqual(0, run.ExitCode);
    }

    private Task<TestApplicationRun> RunThePackingAsync(params string[] groups) =>
        TestApplicationRun.StartAsync(
            new Dictionary<string, string> { ["TF_BUILD"] = "true" },
            testContext.CancellationToken,
            ["--filter", $"ClassName={typeof(PackingTests).FullName}", "--report-azdo", .. groups]);
}
