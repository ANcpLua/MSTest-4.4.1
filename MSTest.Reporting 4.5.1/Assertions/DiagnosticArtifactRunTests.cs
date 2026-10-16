using System.Text.Json;

namespace MSTest.Reporting.Assertions;

[TestClass]
public sealed class DiagnosticArtifactRunTests(TestContext testContext)
{
    [TestMethod]
    public async Task Assertion_diagnostics_retain_values_source_and_test_identity()
    {
        var settings = Path.Combine(testContext.TestTempDirectory!, "diagnostics.json");
        await File.WriteAllTextAsync(settings,
            """{"mstest":{"execution":{"captureAssertionFailureDiagnostics":true}}}""",
            testContext.CancellationToken);
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken,
            "--filter", $"FullyQualifiedName={typeof(FailingAssertionTests).FullName}.{nameof(FailingAssertionTests.Compares_two_numbers)}",
            "--config-file", settings, "--report-trx");

        Assert.AreEqual(2, run.ExitCode);
        var path = Assert.ContainsSingle(Directory.GetFiles(run.Results.FullName,
            "mstest-assertion-failure-state-*.json", SearchOption.AllDirectories));
        using var artifact = JsonDocument.Parse(await File.ReadAllTextAsync(path, testContext.CancellationToken));
        var root = artifact.RootElement;
        Assert.AreEqual(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("15", root.GetProperty("assertion").GetProperty("expected").GetString());
        Assert.AreEqual("12.5", root.GetProperty("assertion").GetProperty("actual").GetString());
        Assert.AreEqual($"{typeof(FailingAssertionTests).FullName}.{nameof(FailingAssertionTests.Compares_two_numbers)}",
            root.GetProperty("test").GetProperty("fullyQualifiedName").GetString());
        var frame = Assert.ContainsSingle(root.GetProperty("stackFrames").EnumerateArray()
            .Where(frame => frame.TryGetProperty("method", out var method)
                && method.GetString()!.Contains(nameof(FailingAssertionTests.Compares_two_numbers), StringComparison.Ordinal)));
        Assert.EndsWith("FailingAssertionTests.cs", frame.GetProperty("file").GetString());
        Assert.AreEqual(26, frame.GetProperty("line").GetInt32());
    }
}
