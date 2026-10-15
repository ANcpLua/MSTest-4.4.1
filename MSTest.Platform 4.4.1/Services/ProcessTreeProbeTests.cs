using System.Diagnostics;
using MSTest.Showcase;

namespace MSTest.Platform.Services;

/// <summary>
/// A process tree for <see cref="ProcessCancellationTests"/> to cancel. Both tests wait until their host
/// is stopped. The parent writes both process IDs after starting its descendant.
/// </summary>
[TestClass]
[Explicit]
public sealed class ProcessTreeProbeTests(TestContext testContext)
{
    internal const string PidFile = "SHOWCASE_PROCESS_TREE_PIDS";

    [TestMethod]
    public async Task Starts_a_descendant_and_waits()
    {
        var start = new ProcessStartInfo(Environment.ProcessPath!,
            ["--filter", $"ClassName={typeof(ProcessTreeProbeTests).FullName}&Name={nameof(Waits_for_its_host_to_stop)}"])
        {
            RedirectStandardOutput = true,
        };
        await using var child = ChildProcess.Start(start, testContext.CancellationToken);
        var output = child.Process.StandardOutput.ReadToEndAsync(testContext.CancellationToken);
        var path = Environment.GetEnvironmentVariable(PidFile)!;
        await File.WriteAllLinesAsync(path + ".tmp",
            [Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                child.Process.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)], testContext.CancellationToken);
        File.Move(path + ".tmp", path);

        await Task.Delay(Timeout.InfiniteTimeSpan, testContext.CancellationToken);
        await output;
    }

    [TestMethod]
    public Task Waits_for_its_host_to_stop() => Task.Delay(Timeout.InfiniteTimeSpan, testContext.CancellationToken);
}
