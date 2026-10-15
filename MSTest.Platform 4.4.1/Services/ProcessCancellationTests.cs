using System.Diagnostics;

namespace MSTest.Platform.Services;

/// <summary>
/// Cancels a real run of this application after it has started a descendant. The process helper must
/// terminate the tree before propagating cancellation to its caller.
/// </summary>
[TestClass]
public sealed class ProcessCancellationTests(TestContext testContext)
{
    [TestMethod]
    public async Task Cancelling_a_run_terminates_its_child_and_descendant()
    {
        var results = Directory.CreateDirectory(Path.Combine(testContext.TestTempDirectory!, "results"));
        var pids = Path.Combine(testContext.TestTempDirectory!, "pids");
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(testContext.CancellationToken);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(testContext.CancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        var running = TestApplicationRun.StartAsync(results,
            new Dictionary<string, string> { [ProcessTreeProbeTests.PidFile] = pids }, cancellation.Token,
            "--filter", $"ClassName={typeof(ProcessTreeProbeTests).FullName}&Name={nameof(ProcessTreeProbeTests.Starts_a_descendant_and_waits)}");

        var processes = new List<Process>();
        try
        {
            while (!File.Exists(pids))
            {
                await Task.Delay(10, deadline.Token);
            }

            foreach (var pid in await File.ReadAllLinesAsync(pids, deadline.Token))
            {
                processes.Add(Process.GetProcessById(int.Parse(pid, System.Globalization.CultureInfo.InvariantCulture)));
            }

            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => running.WaitAsync(deadline.Token));

            Assert.IsTrue(processes[0].HasExited, "The helper must await the child before returning cancellation.");
            await processes[1].WaitForExitAsync(deadline.Token);
            Assert.IsTrue(processes[1].HasExited, "The descendant must also be terminated.");
        }
        finally
        {
            cancellation.Cancel();
            // Keep the regression safe even when the process helper stops terminating children.
            foreach (var process in processes)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }

                    await process.WaitForExitAsync(CancellationToken.None);
                }
                finally
                {
                    process.Dispose();
                }
            }

            try
            {
                using var unexpectedResult = await running;
            }
            catch (OperationCanceledException)
            {
                // The caller owns the results directory when no result record was returned.
            }
        }
    }
}
