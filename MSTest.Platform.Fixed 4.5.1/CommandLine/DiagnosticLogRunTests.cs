using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.CommandLine;

/// <summary>
/// When <c>--diagnostic</c> cannot create its log file, the application ends with an exception that names the
/// file and carries the error of the file system as its inner exception. Here the file name is longer than
/// the file system allows, because <c>--diagnostic-file-prefix</c> has 300 characters. The exception is
/// unhandled, so the process ends with the exit code of an abort, 134 on macOS.
/// </summary>
[TestClass]
public sealed class DiagnosticLogRunTests(TestContext testContext)
{
    [TestMethod]
    public async Task Error_names_the_log_file_and_the_reason()
    {
        var results = Directory.CreateTempSubdirectory();
        var prefix = new string('p', 300);

        using var run = await TestApplicationRun.StartAsync(
            Environment.ProcessPath!,
            results,
            new Dictionary<string, string>(),
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(ShelfTests).FullName}",
            "--diagnostic",
            "--diagnostic-output-directory", results.FullName,
            "--diagnostic-file-prefix", prefix);

        Assert.Contains($"System.IO.IOException: Failed to create the diagnostic log file '{Path.Combine(results.FullName, prefix)}", run.Error);
        Assert.Contains(" ---> System.IO.PathTooLongException: ", run.Error);
        Assert.AreEqual(134, run.ExitCode);
    }
}
