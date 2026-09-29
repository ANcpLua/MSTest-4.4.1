using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.AzureDevOps;

/// <summary>
/// <c>--publish-azdo-test-results</c> publishes the results to a test run of Azure DevOps while the tests run.
/// It needs <c>TF_BUILD</c> set to <c>true</c>, as on an agent of Azure Pipelines, and the variables of the
/// pipeline that name the organization, the project, the build and the access token. When one of them is
/// missing, the report names them on the output of the run, as a warning for Azure Pipelines as well when
/// <c>TF_BUILD</c> is <c>true</c>, publishes nothing and lets the tests run. Here the runs have none of the
/// variables of the pipeline, and the second one sets <c>TF_BUILD</c> itself.
/// </summary>
[TestClass]
public sealed class MissingConfigurationRunTests(TestContext testContext)
{
    private const string Disabled =
        "'--publish-azdo-test-results' was set, but Azure DevOps live publishing is disabled because the following environment variables are missing or invalid: ";

    [TestMethod]
    public async Task Run_outside_a_pipeline_names_every_missing_variable()
    {
        using var run = await PublishTheShelfAsync(new Dictionary<string, string>());
        var lines = run.Output.Split(Environment.NewLine);

        Assert.Contains($"{Disabled}TF_BUILD=true, SYSTEM_COLLECTIONURI, SYSTEM_TEAMPROJECT, SYSTEM_ACCESSTOKEN, BUILD_BUILDID.", lines);
        Assert.Contains("  succeeded: 1", lines);
        Assert.AreEqual(0, run.ExitCode);
    }

    [TestMethod]
    public async Task Run_on_an_agent_warns_the_pipeline_as_well()
    {
        using var run = await PublishTheShelfAsync(new Dictionary<string, string> { ["TF_BUILD"] = "true" });
        var lines = run.Output.Split(Environment.NewLine);
        var warning = $"{Disabled}SYSTEM_COLLECTIONURI, SYSTEM_TEAMPROJECT, SYSTEM_ACCESSTOKEN, BUILD_BUILDID.";

        Assert.Contains(warning, lines);
        Assert.Contains($"##vso[task.logissue type=warning]{warning}", lines);
        Assert.Contains("  succeeded: 1", lines);
        Assert.AreEqual(0, run.ExitCode);
    }

    private Task<TestApplicationRun> PublishTheShelfAsync(IReadOnlyDictionary<string, string> environment) =>
        TestApplicationRun.StartAsync(
            environment,
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(ShelfTests).FullName}",
            "--publish-azdo-test-results");
}
