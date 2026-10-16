using System.Diagnostics;
using System.Xml.Linq;
using MSTest.Showcase;

namespace MSTest.Execution.Timeouts;

[TestClass]
public sealed class FixtureRunTests(TestContext testContext)
{
    [TestMethod]
    [DataRow("default", 1000, 1000)]
    [DataRow("fallback", 600, 700)]
    [DataRow("runsettings", 800, 900)]
    public async Task Global_fixture_timeouts_are_reported_per_test(string settings, int initialize, int cleanup)
    {
        var arguments = settings switch
        {
            "fallback" => new[] { "--config-file", Path.Combine(AppContext.BaseDirectory, "Timeouts/fallback.testconfig.json") },
            "runsettings" => ["--config-file", Path.Combine(AppContext.BaseDirectory, "Timeouts/no-mstest.testconfig.json"),
                "--settings", Path.Combine(AppContext.BaseDirectory, "Timeouts/global-fixtures.runsettings")],
            _ => []
        };
        var (output, report) = await RunAsync(typeof(GlobalFixtureTimeoutTests), arguments);

        Assert.Contains($"BeforeEveryTest' timed out after {initialize}ms", output);
        Assert.Contains($"AfterEveryTest' timed out after {cleanup}ms", output);
        AssertFailedTests(report, 2);
    }

    [TestMethod]
    public async Task Test_timeout_cancels_the_active_global_fixture()
    {
        var (output, report) = await RunAsync(typeof(GlobalFixtureCancellationTests), []);

        Assert.Contains("BeforeEveryTest' was canceled", output);
        Assert.Contains("AfterEveryTest' was canceled", output);
        AssertFailedTests(report, 2);
    }

    private async Task<(string Output, XDocument Report)> RunAsync(Type fixture, string[] arguments)
    {
        var results = Directory.CreateTempSubdirectory();
        try
        {
            ProcessStartInfo start = new(Environment.ProcessPath!, ["--filter", $"ClassName={fixture.FullName}",
                "--report-trx", "--report-trx-filename", "result.trx", "--results-directory", results.FullName, .. arguments])
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment = { [ExplicitAttribute.Variable] = "1", ["TESTINGPLATFORM_UI_LANGUAGE"] = "en-us" }
            };
            await using var child = ChildProcess.Start(start, testContext.CancellationToken);
            var stdout = child.Process.StandardOutput.ReadToEndAsync(testContext.CancellationToken);
            var stderr = child.Process.StandardError.ReadToEndAsync(testContext.CancellationToken);
            await child.Process.WaitForExitAsync(testContext.CancellationToken);
            Assert.AreEqual(2, child.Process.ExitCode, await stdout + await stderr);
            return (await stdout + await stderr, XDocument.Load(Path.Combine(results.FullName, "result.trx")));
        }
        finally
        {
            results.Delete(true);
        }
    }

    private static void AssertFailedTests(XDocument report, int count)
    {
        XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var results = report.Descendants(trx + "UnitTestResult").ToArray();
        Assert.HasCount(count, results);
        Assert.IsTrue(results.All(result => (string?)result.Attribute("outcome") == "Failed"));
        Assert.HasCount(count, results.Select(result => (string?)result.Attribute("testName")).Distinct());
    }
}
