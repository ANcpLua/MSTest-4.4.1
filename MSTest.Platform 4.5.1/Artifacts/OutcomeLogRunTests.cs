using System.Diagnostics;
using System.Text.Json;
using Microsoft.Testing.Platform.Extensions.ArtifactPostProcessing;
using MSTest.Platform.Filtering;
using MSTest.Platform.Retries;
using MSTest.Platform.Terminal;
using MSTest.Showcase;

namespace MSTest.Platform.Artifacts;

/// <summary>
/// Runs the registered outcome logger in a separate process, then merges its artifacts. Reading nonempty
/// logs also verifies that the composite registration shares the consumer with the session handler.
/// </summary>
[TestClass]
public sealed class OutcomeLogRunTests(TestContext testContext)
{
    [TestMethod]
    public async Task Session_artifact_contains_the_final_outcome_of_each_test()
    {
        using var run = await RunRoutesAsync();
        var outcomes = JsonSerializer.Deserialize<OutcomeLog.Entry[]>(await File.ReadAllTextAsync(
            run.PathOf(OutcomeLog.FileName), testContext.CancellationToken))!;

        Assert.AreEqual(0, run.ExitCode);
        Assert.AreSequenceEqual(
            [nameof(RouteTests.Plans_a_route_by_road), nameof(RouteTests.Plans_a_route_by_rail), nameof(RouteTests.Plans_a_route_by_sea)],
            outcomes.Select(static entry => entry.Name), SequenceOrder.InAnyOrder);
        Assert.HasCount(3, outcomes.Select(static entry => entry.Uid).Distinct());
        Assert.AreSequenceEqual(["passed"],
            outcomes.Select(static entry => entry.State).Distinct());
        Assert.Contains(OutcomeLog.FileName, run.Output);
    }

    [TestMethod]
    public async Task Merger_preserves_every_input_and_combines_all_their_outcomes()
    {
        using var first = await RunRoutesAsync();
        using var second = await RunRoutesAsync();
        var firstJson = await File.ReadAllTextAsync(first.PathOf(OutcomeLog.FileName), testContext.CancellationToken);
        var secondJson = await File.ReadAllTextAsync(second.PathOf(OutcomeLog.FileName), testContext.CancellationToken);
        var merged = await new OutcomeLogMerger().ProcessAsync(
            [Input(first), Input(second)], Path.Combine(testContext.TestTempDirectory!, "merged"),
            new ArtifactPostProcessingContext(ArtifactPostProcessingTruncationReason.Timeout), testContext.CancellationToken);

        Assert.IsNotNull(merged);
        Assert.AreEqual(OutcomeLog.Kind, merged.Kind);
        var outcomes = JsonSerializer.Deserialize<OutcomeLog.Entry[]>(await File.ReadAllTextAsync(merged.Path, testContext.CancellationToken))!;
        Assert.HasCount(6, outcomes);
        Assert.AreSequenceEqual(
            JsonSerializer.Deserialize<OutcomeLog.Entry[]>(firstJson)!.Concat(JsonSerializer.Deserialize<OutcomeLog.Entry[]>(secondJson)!),
            outcomes);
        Assert.AreEqual(firstJson, await File.ReadAllTextAsync(first.PathOf(OutcomeLog.FileName), testContext.CancellationToken));
        Assert.AreEqual(secondJson, await File.ReadAllTextAsync(second.PathOf(OutcomeLog.FileName), testContext.CancellationToken));
    }

    [TestMethod]
    public async Task Malformed_input_is_rejected_without_overwriting_it()
    {
        var path = Path.Combine(testContext.TestTempDirectory!, OutcomeLog.FileName);
        const string partialJson = "[{";
        await File.WriteAllTextAsync(path, partialJson, testContext.CancellationToken);
        var merger = new OutcomeLogMerger();

        await Assert.ThrowsExactlyAsync<JsonException>(() => merger.ProcessAsync(
            [new InputArtifact(path, OutcomeLog.Kind, null, null, null, null)],
            Path.Combine(testContext.TestTempDirectory!, "merged"),
            new ArtifactPostProcessingContext(ArtifactPostProcessingTruncationReason.None), testContext.CancellationToken));

        Assert.AreEqual(partialJson, await File.ReadAllTextAsync(path, testContext.CancellationToken));
    }

    [TestMethod]
    public async Task Retried_tests_have_one_final_passed_outcome_each()
    {
        using var run = await TestApplicationRun.StartAsync(
            new Dictionary<string, string?> { [OutcomeLog.Variable] = "1" }, testContext.CancellationToken,
            "--filter", $"ClassName={typeof(UnsteadyTests).FullName}");
        var outcomes = JsonSerializer.Deserialize<OutcomeLog.Entry[]>(await File.ReadAllTextAsync(
            run.PathOf(OutcomeLog.FileName), testContext.CancellationToken))!;

        Assert.AreEqual(0, run.ExitCode);
        Assert.HasCount(2, outcomes);
        Assert.AreSequenceEqual(["passed", "passed"], outcomes.Select(static entry => entry.State));
        Assert.AreSequenceEqual(
            [nameof(UnsteadyTests.Scans_a_label_at_the_third_attempt), nameof(UnsteadyTests.Scans_a_label_at_once)],
            outcomes.Select(entry => entry.Name), SequenceOrder.InAnyOrder);
        Assert.HasCount(2, outcomes.Select(entry => entry.Uid).Distinct());
    }

    [TestMethod]
    public async Task Outcomes_use_literal_states_without_diagnostic_explanations()
    {
        using var run = await TestApplicationRun.StartAsync(
            new Dictionary<string, string?> { [OutcomeLog.Variable] = "1" }, testContext.CancellationToken,
            "--filter", $"ClassName={typeof(ShipmentTests).FullName}");
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(
            run.PathOf(OutcomeLog.FileName), testContext.CancellationToken));

        Assert.AreEqual(2, run.ExitCode);
        Assert.AreSequenceEqual(
            [
                $"{nameof(ShipmentTests.Ships_a_paid_order)}:passed",
                $"{nameof(ShipmentTests.Ships_an_order_that_costs_more_than_was_paid)}:failed",
                $"{nameof(ShipmentTests.Waits_for_a_carrier_that_never_answers)}:failed",
                $"{nameof(ShipmentTests.Ships_abroad)}:skipped"
            ],
            document.RootElement.EnumerateArray().Select(static entry => $"{entry.GetProperty("Name").GetString()}:{entry.GetProperty("State").GetString()}"),
            SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public async Task Repeating_a_merge_reuses_its_identical_output()
    {
        using var run = await RunRoutesAsync();
        var directory = Path.Combine(testContext.TestTempDirectory!, "merged");
        var context = new ArtifactPostProcessingContext(ArtifactPostProcessingTruncationReason.None);
        var merger = new OutcomeLogMerger();
        var first = await merger.ProcessAsync([Input(run)], directory, context, testContext.CancellationToken);
        Assert.IsNotNull(first);
        var contents = await File.ReadAllTextAsync(first.Path, testContext.CancellationToken);

        var second = await merger.ProcessAsync([Input(run)], directory, context, testContext.CancellationToken);

        Assert.IsNotNull(second);
        Assert.AreEqual(first.Path, second.Path);
        Assert.AreEqual(contents, await File.ReadAllTextAsync(second.Path, testContext.CancellationToken));
        Assert.HasCount(1, Directory.EnumerateFiles(directory));
    }

    [TestMethod]
    [OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
    [DataRow("directory")]
    [DataRow("file")]
    [DataRow("hard")]
    public async Task Filesystem_aliases_cannot_make_a_merge_overwrite_an_input(string linkKind)
    {
        var source = Directory.CreateDirectory(Path.Combine(testContext.TestTempDirectory!, "source"));
        var input = Path.Combine(source.FullName, OutcomeLog.FileName);
        const string json = "[ { \"Uid\": \"one\", \"Name\": \"Example\", \"State\": \"passed\" } ]";
        await File.WriteAllTextAsync(input, json, testContext.CancellationToken);
        var output = Path.Combine(testContext.TestTempDirectory!, "alias");
        if (linkKind == "directory")
        {
            Directory.CreateSymbolicLink(output, source.FullName);
        }
        else
        {
            Directory.CreateDirectory(output);
            var destination = Path.Combine(output, OutcomeLog.FileName);
            if (linkKind == "file")
            {
                File.CreateSymbolicLink(destination, input);
            }
            else
            {
                await using var link = ChildProcess.Start(new ProcessStartInfo("ln", [input, destination]), testContext.CancellationToken);
                await link.Process.WaitForExitAsync(testContext.CancellationToken);
                Assert.AreEqual(0, link.Process.ExitCode);
            }
        }

        try
        {
            var merger = new OutcomeLogMerger();
            await Assert.ThrowsExactlyAsync<IOException>(() => merger.ProcessAsync(
                [new InputArtifact(input, OutcomeLog.Kind, null, null, null, null)], output,
                new ArtifactPostProcessingContext(ArtifactPostProcessingTruncationReason.None), testContext.CancellationToken));

            Assert.AreEqual(json, await File.ReadAllTextAsync(input, testContext.CancellationToken));
            Assert.HasCount(1, Directory.EnumerateFiles(output));
        }
        finally
        {
            // Remove the link before TestTempDirectory cleans up the source directory.
            if (linkKind == "directory")
            {
                Directory.Delete(output);
            }
            else
            {
                File.Delete(Path.Combine(output, OutcomeLog.FileName));
            }
        }
    }

    private Task<TestApplicationRun> RunRoutesAsync() => TestApplicationRun.StartAsync(
        new Dictionary<string, string?> { [OutcomeLog.Variable] = "1" }, testContext.CancellationToken,
        "--filter", $"ClassName={typeof(RouteTests).FullName}");

    private static InputArtifact Input(TestApplicationRun run) =>
        new(run.PathOf(OutcomeLog.FileName), OutcomeLog.Kind, null, null, null, null);
}
