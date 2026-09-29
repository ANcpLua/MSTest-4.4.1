using System.Text.RegularExpressions;

namespace MSTest.Platform.Changed.SlowTests;

/// <summary>
/// The line of the Azure DevOps report about a test that is still running names the row of a data-driven
/// test: the full name of the method, followed by the arguments of the row. The line needs
/// <c>--report-azdo</c>, <c>--report-azdo-slow-test-history</c> with a number of days and the variable
/// <c>TF_BUILD</c> set to <c>true</c>, which this example sets itself, as an agent of Azure Pipelines does.
/// Without the history of Azure DevOps, which this run cannot reach, a test is slow after one minute. The
/// options need <c>EnableMicrosoftTestingExtensionsAzureDevOpsReport</c> in the project file.
/// </summary>
[TestClass]
public sealed class AzureDevOpsLineRunTests
{
    private static readonly string Method = $"{typeof(DeliveryTests).FullName}.{nameof(DeliveryTests.Arrives_by_ship)}";

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheDeliveryByShip(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            new Dictionary<string, string> { ["TF_BUILD"] = "true" },
            testContext.CancellationToken,
            "--filter", $"FullyQualifiedName={Method}",
            "--report-azdo",
            "--report-azdo-slow-test-history", "30");

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    public void Line_names_the_row_of_the_slow_test() =>
        Assert.MatchesRegex($@"(?m)^\[slow\] still running after 1(\.\d)?m: {Regex.Escape($"{Method} (63)")}$", _run.Output);

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);
}
