using System.Text.RegularExpressions;

namespace MSTest.Platform.Changed.SlowTests;

/// <summary>
/// The notice of the GitHub Actions report about a test that is still running names the row of a data-driven
/// test: the full name of the method, followed by the arguments of the row. The report is active with
/// <c>--report-gh</c> and the variable <c>GITHUB_ACTIONS</c> set to <c>true</c>, which this example sets
/// itself, as a runner of GitHub Actions does. <c>--report-gh-slow-test-threshold</c> sets the time after
/// which a test is slow. The option needs <c>EnableMicrosoftTestingExtensionsGitHubActionsReport</c> in the
/// project file.
/// </summary>
[TestClass]
public sealed class GitHubNoticeRunTests
{
    private static readonly string Method = $"{typeof(DeliveryTests).FullName}.{nameof(DeliveryTests.Arrives_by_bike)}";

    private static TestApplicationRun _run = null!;

    [ClassInitialize]
    public static async Task RunTheDeliveriesByBike(TestContext testContext) =>
        _run = await TestApplicationRun.StartAsync(
            new Dictionary<string, string> { ["GITHUB_ACTIONS"] = "true" },
            testContext.CancellationToken,
            "--filter", $"FullyQualifiedName={Method}",
            "--report-gh",
            "--report-gh-step-summary", "off",
            "--report-gh-slow-test-threshold", "1s");

    [ClassCleanup]
    public static void DeleteTheResults() => _run.Dispose();

    [TestMethod]
    [DataRow(3)]
    [DataRow(4)]
    public void Notice_names_the_row_of_the_slow_test(int seconds)
    {
        var row = Regex.Escape($"{Method} ({seconds})");

        Assert.MatchesRegex($@"(?m)^::notice title=Slow test%3A {row}::{row} still running after \d+s$", _run.Output);
    }

    [TestMethod]
    public void Run_passes() => Assert.AreEqual(0, _run.ExitCode);
}
