using System.Text.Json;
using Microsoft.Testing.Platform.Extensions.ArtifactPostProcessing;
using Microsoft.Testing.Platform.Extensions.Messages;
using MSTest.Platform.Filtering;

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
        Assert.AreSequenceEqual([PassedTestNodeStateProperty.CachedInstance.ToString()],
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

    private Task<TestApplicationRun> RunRoutesAsync() => TestApplicationRun.StartAsync(
        new Dictionary<string, string> { [OutcomeLog.Variable] = "1" }, testContext.CancellationToken,
        "--filter", $"ClassName={typeof(RouteTests).FullName}");

    private static InputArtifact Input(TestApplicationRun run) =>
        new(run.PathOf(OutcomeLog.FileName), OutcomeLog.Kind, null, null, null, null);
}
