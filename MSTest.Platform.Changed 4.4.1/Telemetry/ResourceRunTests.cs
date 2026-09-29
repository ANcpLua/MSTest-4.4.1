using System.Runtime.InteropServices;

namespace MSTest.Platform.Changed.Telemetry;

/// <summary>
/// The resource that the provider attaches to every span and instrument describes where the run happened.
/// <c>AddTestingPlatformResource</c>, which <c>AddOpenTelemetryProviderFromEnvironment</c> calls, detects the
/// test assembly, the host, the operating system and the runtime, and names the service after the test
/// assembly. On a CI system it adds the provider, the pipeline, the branch and the commit from the variables
/// of that system. This example sets the variables of GitHub Actions itself, as a runner does.
/// </summary>
[TestClass]
public sealed class ResourceRunTests(TestContext testContext)
{
    private static readonly string Assembly = typeof(ResourceRunTests).Assembly.GetName().Name!;

    [TestMethod]
    public async Task Resource_names_the_assembly_the_host_and_the_runtime()
    {
        var resource = await DetectAsync([]);

        Assert.AreEqual(Assembly, resource["service.name"]);
        Assert.AreEqual(Assembly, resource["test.assembly.name"]);
        Assert.AreEqual(Environment.MachineName, resource["host.name"]);
        Assert.AreEqual(RuntimeInformation.OSDescription, resource["os.description"]);
        Assert.AreEqual(".NET", resource["process.runtime.name"]);
        Assert.AreEqual(RuntimeInformation.FrameworkDescription, resource["process.runtime.description"]);
        Assert.AreNotEqual(Environment.ProcessId, int.Parse(resource["process.pid"]!));
    }

    [TestMethod]
    public async Task Resource_has_no_pipeline_outside_a_CI_system() =>
        Assert.DoesNotContain("cicd.provider.name", (await DetectAsync([])).Keys);

    [TestMethod]
    public async Task Resource_names_the_pipeline_the_branch_and_the_commit_of_GitHub_Actions()
    {
        var resource = await DetectAsync(new()
        {
            ["GITHUB_ACTIONS"] = "true",
            ["GITHUB_WORKFLOW"] = "build",
            ["GITHUB_RUN_ID"] = "42",
            ["GITHUB_JOB"] = "test",
            ["GITHUB_REF_NAME"] = "main",
            ["GITHUB_SHA"] = "0123abc",
            ["GITHUB_REPOSITORY"] = "shop/checkout",
        });

        Assert.AreEqual("github_actions", resource["cicd.provider.name"]);
        Assert.AreEqual("build", resource["cicd.pipeline.name"]);
        Assert.AreEqual("42", resource["cicd.pipeline.run.id"]);
        Assert.AreEqual("test", resource["cicd.pipeline.task.name"]);
        Assert.AreEqual("main", resource["vcs.ref.head.name"]);
        Assert.AreEqual("0123abc", resource["vcs.ref.head.revision"]);
        Assert.AreEqual("shop/checkout", resource["vcs.repository.name"]);
    }

    private async Task<IReadOnlyDictionary<string, string?>> DetectAsync(Dictionary<string, string> environment)
    {
        using var run = await TestApplicationRun.StartAsync(
            environment,
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(CheckoutTests).FullName}");

        return Assert.ContainsSingle(run.ReadSpans().Select(static span => span.Resource).DistinctBy(static resource => resource["service.instance.id"]));
    }
}
