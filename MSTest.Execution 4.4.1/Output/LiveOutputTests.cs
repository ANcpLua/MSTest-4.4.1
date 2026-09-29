using System.Diagnostics;

namespace MSTest.Execution.Output;

/// <summary>
/// testconfig.json sets <c>mstest:output:captureTrace</c> to <c>Live</c>: a line that a test writes through
/// <see cref="Console"/>, <see cref="Trace"/> or <see cref="TestContext"/> is on the standard output of the test
/// application while the test is still running, and a line for <see cref="Console.Error"/> is on its standard
/// error. Each test here starts this test application for one test of <see cref="ImportProgress"/>, reads the
/// line from that output and only then answers on the standard input, for which the started test waits.
/// </summary>
[TestClass]
public sealed class LiveOutputTests(TestContext testContext)
{
    [TestMethod]
    [Timeout(30_000, CooperativeCancellation = true)]
    [DataRow(nameof(ImportProgress.Written_to_the_console), ImportProgress.ConsoleLine)]
    [DataRow(nameof(ImportProgress.Written_to_the_trace), ImportProgress.TraceLine)]
    [DataRow(nameof(ImportProgress.Written_to_the_test_context), ImportProgress.TestContextLine)]
    public async Task The_line_is_on_the_standard_output_before_its_test_finishes(string test, string line)
    {
        using Process application = Start(test);

        string? arrived = await ReadUntilAsync(application.StandardOutput, line);
        Assert.AreEqual(line, arrived);

        await application.StandardInput.WriteLineAsync();
        await application.WaitForExitAsync(testContext.CancellationToken);
        Assert.AreEqual(0, application.ExitCode);
    }

    [TestMethod]
    [Timeout(30_000, CooperativeCancellation = true)]
    public async Task The_line_is_on_the_standard_error_before_its_test_finishes()
    {
        using Process application = Start(nameof(ImportProgress.Written_to_the_error_stream));

        string? arrived = await ReadUntilAsync(application.StandardError, ImportProgress.ErrorLine);
        Assert.AreEqual(ImportProgress.ErrorLine, arrived);

        await application.StandardInput.WriteLineAsync();
        await application.WaitForExitAsync(testContext.CancellationToken);
        Assert.AreEqual(0, application.ExitCode);
    }

    private static Process Start(string test)
    {
        ProcessStartInfo application = new(Environment.ProcessPath!)
        {
            ArgumentList = { "--filter", $"FullyQualifiedName={typeof(ImportProgress).FullName}.{test}" },
            Environment = { [InteractiveAttribute.Variable] = "1" },
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        return Process.Start(application)!;
    }

    private async Task<string?> ReadUntilAsync(StreamReader output, string line)
    {
        string? read;
        do
        {
            read = await output.ReadLineAsync(testContext.CancellationToken);
        }
        while (read is not null && read != line);

        return read;
    }
}
