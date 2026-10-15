using System.Diagnostics;
using MSTest.Platform.Terminal;

namespace MSTest.Platform.Pipes;

/// <summary>
///     <c>TESTINGPLATFORM_PIPE_DIRECTORY</c> names the directory in which the platform creates the socket files
///     of its named pipes on Unix, which are in the temporary directory otherwise. The platform creates the
///     directory when it is missing and ends the application when it cannot write to it or when the path of a
///     socket in it is longer than 103 bytes. The runs below use pipes because <c>--report-trx</c> starts the
///     test host as a process of its own, which its controller talks to through pipes. The variable has no
///     effect on Windows, where a pipe is no file.
/// </summary>
[TestClass]
[OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
public sealed class PipeDirectoryRunTests(TestContext testContext)
{
    private const string Variable = "TESTINGPLATFORM_PIPE_DIRECTORY";

    private static readonly string[] StockCountsWithReport =
        ["--filter", $"ClassName={typeof(StockCountTests).FullName}", "--report-trx"];

    [TestMethod]
    public async Task Sockets_of_the_run_are_files_in_the_directory()
    {
        var pipes = Path.Combine("/tmp", Path.GetRandomFileName());
        var results = Directory.CreateTempSubdirectory();
        var start = new ProcessStartInfo(Environment.ProcessPath!,
            [.. StockCountsWithReport, "--results-directory", results.FullName])
        {
            RedirectStandardOutput = true,
            Environment =
            {
                [Variable] = pipes
            }
        };
        var sockets = new HashSet<string>();

        using var application = Process.Start(start)!;
        var output = application.StandardOutput.ReadToEndAsync(testContext.CancellationToken);

        while (!application.HasExited)
        {
            if (Directory.Exists(pipes)) sockets.UnionWith(Directory.EnumerateFiles(pipes));

            await Task.Delay(TimeSpan.FromMilliseconds(10), testContext.CancellationToken);
        }

        await output;
        Assert.IsNotEmpty(sockets);
        Assert.AreEqual(0, application.ExitCode);
        Assert.IsEmpty(Directory.EnumerateFiles(pipes));

        Directory.Delete(pipes);
        results.Delete(true);
    }

    [TestMethod]
    public async Task Directory_that_cannot_be_created_ends_the_application()
    {
        var file = Path.GetTempFileName();
        var below = Path.Combine(file, "pipes");

        using var run = await RunAsync(below);

        Assert.Contains($"The named pipe directory '{below}' does not exist or is not writable", run.Error);
        Assert.Contains($"Set the '{Variable}' environment variable to an existing writable directory", run.Error);
        Assert.AreNotEqual(0, run.ExitCode);

        File.Delete(file);
    }

    [TestMethod]
    public async Task Directory_whose_sockets_have_too_long_a_path_ends_the_application()
    {
        var deep = Path.Combine("/tmp", Path.GetRandomFileName(), new string('p', 60));

        using var run = await RunAsync(deep);

        Assert.MatchesRegex(
            @"The named pipe path '.+' is \d+ bytes long, which exceeds the maximum of 103 bytes allowed for a Unix domain socket\.",
            run.Error);
        Assert.AreNotEqual(0, run.ExitCode);

        Directory.Delete(Path.GetDirectoryName(deep)!, true);
    }

    private Task<TestApplicationRun> RunAsync(string pipes)
    {
        return TestApplicationRun.StartAsync(new Dictionary<string, string> { [Variable] = pipes },
            testContext.CancellationToken, StockCountsWithReport);
    }
}