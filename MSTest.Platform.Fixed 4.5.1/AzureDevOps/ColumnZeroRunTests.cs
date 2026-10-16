using MSTest.Platform.Fixed.MSBuild;
using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.AzureDevOps;

/// <summary>
///     Azure Pipelines reads a logging command such as <c>##vso[task.logissue]</c> or <c>##[group]</c> only at the
///     start of a line. When the MSBuild target <c>InvokeTestingPlatform</c> runs the test application, the Azure
///     DevOps report writes its commands at column zero of the output of MSBuild, where MSBuild indents the other
///     lines of the application. The report is active with <c>--report-azdo</c> and the variable <c>TF_BUILD</c>
///     set to <c>true</c>, which this example sets itself, as an agent of Azure Pipelines does. The commands of a
///     build with several MSBuild nodes (<c>dotnet test -m</c>) do not reach the output. Here the target runs
///     <see cref="DamagedParcelTests.Rejects_the_damaged_parcel" /> with <c>--report-azdo-groups on</c>.
/// </summary>
[TestClass]
public sealed class ColumnZeroRunTests
{
    private static TestTargetRun _run = null!;
    private static string[] _lines = null!;

    [ClassInitialize]
    public static async Task RunTheDamagedParcelThroughMsBuild(TestContext testContext)
    {
        _run = await TestTargetRun.StartAsync(
            $"--filter FullyQualifiedName={typeof(DamagedParcelTests).FullName}.{nameof(DamagedParcelTests.Rejects_the_damaged_parcel)} --report-azdo --report-azdo-groups on",
            new Dictionary<string, string>(),
            new Dictionary<string, string> { ["TF_BUILD"] = "true" },
            testContext.CancellationToken);
        _lines = _run.Output.Split(Environment.NewLine);
    }

    [TestMethod]
    public void Issue_of_the_failed_test_starts_at_column_zero()
    {
        Assert.ContainsSingle(_lines.Where(static line =>
            line.StartsWith("##vso[task.logissue type=error", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void Group_of_the_assembly_starts_and_ends_at_column_zero()
    {
        Assert.ContainsSingle(
            _lines.Where(static line => line.StartsWith("##[group]Tests: ", StringComparison.Ordinal)));
        Assert.Contains("##[endgroup]", _lines);
    }

    [TestMethod]
    public void Other_lines_of_the_application_are_indented_by_MSBuild()
    {
        Assert.ContainsSingle(_lines.Where(static line =>
            line.StartsWith("  failed Rejects_the_damaged_parcel", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void Target_fails_with_the_test()
    {
        Assert.AreNotEqual(0, _run.ExitCode);
    }
}