using MSTest.Platform.Fixed.Warehouse;

namespace MSTest.Platform.Fixed.Retries;

/// <summary>
/// Options can come from a response file, named on the command line with <c>@</c>, and a response file can
/// name another one. When <c>--retry-failed-tests</c> starts the test application again, each attempt gets the
/// options of all response files, here the filter and the retry count of two nested files around
/// <see cref="RestockTests"/>. A nested response file is read from the working directory of the process, not
/// from the directory of the file that names it, and a file that does not exist is named in the error of the
/// run.
/// </summary>
[TestClass]
public sealed class ResponseFileRunTests(TestContext testContext)
{
    private const int InvalidCommandLine = 5;

    [TestMethod]
    public async Task Retry_runs_the_options_of_nested_response_files_again()
    {
        var results = Directory.CreateTempSubdirectory();
        var filter = Path.Combine(results.FullName, "filter.rsp");
        var retry = Path.Combine(results.FullName, "retry.rsp");
        await File.WriteAllLinesAsync(filter, ["--filter", $"ClassName={typeof(RestockTests).FullName}"], testContext.CancellationToken);
        await File.WriteAllLinesAsync(retry, ["--retry-failed-tests 1", $"@{filter}"], testContext.CancellationToken);

        using var run = await TestApplicationRun.StartAsync(
            Environment.ProcessPath!,
            results,
            new Dictionary<string, string> { [RestockTests.Shelf] = results.CreateSubdirectory("shelf").FullName },
            testContext.CancellationToken,
            $"@{retry}");

        Assert.Contains("Retry: attempt 1/2 failed - 1 failing test(s), retrying", run.Output);
        Assert.Contains("Retry summary: Passed! after 2/2 attempts", run.Output);
        Assert.Contains("  total: 2", run.Output);
        Assert.AreEqual(0, run.ExitCode);
    }

    [TestMethod]
    public async Task Missing_nested_response_file_is_named_in_the_error()
    {
        var results = Directory.CreateTempSubdirectory();
        var outer = Path.Combine(results.FullName, "outer.rsp");
        var missing = Path.Combine(results.FullName, "missing.rsp");
        await File.WriteAllLinesAsync(outer, ["--retry-failed-tests 1", $"@{missing}"], testContext.CancellationToken);

        using var run = await TestApplicationRun.StartAsync(
            Environment.ProcessPath!,
            results,
            new Dictionary<string, string>(),
            testContext.CancellationToken,
            $"@{outer}");

        Assert.Contains($"The response file '{missing}' was not found", run.Output);
        Assert.AreEqual(InvalidCommandLine, run.ExitCode);
    }
}
