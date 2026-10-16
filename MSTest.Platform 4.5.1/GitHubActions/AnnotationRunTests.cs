using System.Runtime.CompilerServices;
using MSTest.Platform.Terminal;

namespace MSTest.Platform.GitHubActions;

/// <summary>
/// Where the GitHub Actions report places its annotations in the files of the workspace. A failure with a
/// stack frame in the workspace gets an <c>::error</c> at the line of that frame, here line 20 of
/// <see cref="ShipmentTests"/>, the assertion. A failure without such a frame, the test that exceeds its
/// timeout, gets its <c>::error</c> at the declaration of the test, line 22, which MSTest reports as the
/// location of the test. A skipped test gets a <c>::warning</c> at its declaration, line 26, with the reason
/// of the skip as its message. The report is active with <c>--report-gh</c> and the variable
/// <c>GITHUB_ACTIONS</c> set to <c>true</c>, and paths are relative to <c>GITHUB_WORKSPACE</c>. This example
/// sets both variables itself, as a runner of GitHub Actions does, with the root of this repository as the
/// workspace. The option needs <c>EnableMicrosoftTestingExtensionsGitHubActionsReport</c> in the project file.
/// </summary>
[TestClass]
public sealed class AnnotationRunTests(TestContext testContext)
{
    private const string File = "MSTest.Platform 4.5.1/Terminal/ShipmentTests.cs";
    private const string Title = "title=Test failed%3A MSTest.Platform.Terminal.ShipmentTests";

    [TestMethod]
    public async Task Failure_with_a_stack_frame_is_annotated_at_the_frame()
    {
        using var run = await RunTheShipmentsAsync();

        Assert.Contains(
            $"::error file={File},line=20,col=1,{Title}.{nameof(ShipmentTests.Ships_an_order_that_costs_more_than_was_paid)}::Assertion failed.",
            run.Output);
    }

    [TestMethod]
    public async Task Failure_without_a_stack_frame_is_annotated_at_the_declaration_of_the_test()
    {
        using var run = await RunTheShipmentsAsync();

        Assert.Contains(
            $"::error file={File},line=22,col=1,{Title}.{nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)}::Test '{nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)}' timed out after 100ms",
            run.Output);
    }

    [TestMethod]
    public async Task Skipped_test_gets_a_warning_at_its_declaration()
    {
        using var run = await RunTheShipmentsAsync();

        Assert.Contains(
            $"::warning file={File},line=26,col=1,title=Test skipped%3A MSTest.Platform.Terminal.ShipmentTests.{nameof(ShipmentTests.Ships_abroad)}::The carrier has no route abroad",
            run.Output);
    }

    [TestMethod]
    public async Task Annotations_that_are_off_leave_the_output_without_them()
    {
        using var run = await RunTheShipmentsAsync("--report-gh-annotations", "off");

        Assert.AreEqual(2, run.ExitCode, run.Output);
        Assert.Contains($"failed {nameof(ShipmentTests.Ships_an_order_that_costs_more_than_was_paid)}", run.ResultLines);
        Assert.DoesNotContain("::error", run.Output);
        Assert.DoesNotContain("::warning", run.Output);
    }

    private Task<TestApplicationRun> RunTheShipmentsAsync(params string[] options) =>
        TestApplicationRun.StartAsync(
            new Dictionary<string, string?> { ["GITHUB_ACTIONS"] = "true", ["GITHUB_WORKSPACE"] = RepositoryRoot() },
            testContext.CancellationToken,
            ["--filter", $"ClassName={typeof(ShipmentTests).FullName}", "--report-gh", .. options]);

    private static string RepositoryRoot([CallerFilePath] string file = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, "..", ".."));
}
