using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Terminal;

/// <summary>
///     A <c>commandLineOptionDefaults</c> section in <c>testconfig.json</c> gives an option a default value without
///     turning the option on. The terminal reporter of a test application that is started directly takes the
///     default of <c>--show-test-results</c>, which chooses the outcomes whose results it prints, and a
///     <c>--show-test-results</c> on the command line wins over the default. Here the configuration file, passed with
///     <c>--config-file</c>, asks for passed results only, and each run has
///     <see cref="ShelfTests.Counts_the_boxes_on_the_shelf" />, which passes, and
///     <see cref="DamagedParcelTests.Rejects_the_damaged_parcel" />, which fails.
/// </summary>
[TestClass]
public sealed class PassiveDefaultRunTests(TestContext testContext)
{
    private const string Passed = $"passed {nameof(ShelfTests.Counts_the_boxes_on_the_shelf)}";
    private const string Failed = $"failed {nameof(DamagedParcelTests.Rejects_the_damaged_parcel)}";

    [TestMethod]
    public async Task Terminal_prints_the_outcomes_of_the_default()
    {
        using var run = await RunTheShelfAndTheParcelAsync();

        Assert.Contains(Passed, run.Output);
        Assert.DoesNotContain(Failed, run.Output);
        Assert.Contains("  failed: 1", run.Output);
    }

    [TestMethod]
    public async Task Command_line_wins_over_the_default()
    {
        using var run = await RunTheShelfAndTheParcelAsync("--show-test-results", "failed");

        Assert.Contains(Failed, run.Output);
        Assert.DoesNotContain(Passed, run.Output);
    }

    private async Task<TestApplicationRun> RunTheShelfAndTheParcelAsync(params string[] options)
    {
        var results = Directory.CreateTempSubdirectory();
        var configuration = Path.Combine(results.FullName, "passed.testconfig.json");
        await File.WriteAllTextAsync(
            configuration,
            """{ "commandLineOptionDefaults": { "show-test-results": [ "passed" ] } }""",
            testContext.CancellationToken);

        return await TestApplicationRun.StartAsync(
            Environment.ProcessPath!,
            results,
            new Dictionary<string, string>(),
            testContext.CancellationToken,
            [
                "--filter",
                $"FullyQualifiedName={typeof(ShelfTests).FullName}.{nameof(ShelfTests.Counts_the_boxes_on_the_shelf)}|FullyQualifiedName={typeof(DamagedParcelTests).FullName}.{nameof(DamagedParcelTests.Rejects_the_damaged_parcel)}",
                "--config-file", configuration,
                .. options
            ]);
    }
}