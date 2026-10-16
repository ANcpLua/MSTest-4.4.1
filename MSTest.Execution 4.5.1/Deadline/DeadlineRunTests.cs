using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using MSTest.Showcase;

namespace MSTest.Execution.Deadline;

[TestClass]
public sealed class DeadlineRunTests(TestContext testContext)
{
    [TestMethod]
    [Timeout(30_000, CooperativeCancellation = true)]
    public async Task Deadline_stops_before_the_next_test_and_keeps_the_finished_result()
    {
        var results = Directory.CreateTempSubdirectory();
        try
        {
            ProcessStartInfo start = new(Environment.ProcessPath!, ["--filter", $"ClassName={typeof(DeadlineTests).FullName}",
                "--report-trx", "--report-trx-filename", "result.trx", "--results-directory", results.FullName])
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment =
                {
                    [ExplicitAttribute.Variable] = "1",
                    ["TESTINGPLATFORM_DEADLINE"] = DateTimeOffset.UtcNow.AddSeconds(10).ToString("O", CultureInfo.InvariantCulture),
                    ["TESTINGPLATFORM_DEADLINE_STOP_MARGIN"] = "7",
                    ["TESTINGPLATFORM_UI_LANGUAGE"] = "en-us"
                }
            };
            await using var child = ChildProcess.Start(start, testContext.CancellationToken);
            var output = child.Process.StandardOutput.ReadToEndAsync(testContext.CancellationToken);
            var error = child.Process.StandardError.ReadToEndAsync(testContext.CancellationToken);
            await child.Process.WaitForExitAsync(testContext.CancellationToken);
            Assert.AreEqual(15, child.Process.ExitCode, await output + await error);
            Assert.Contains("Deadline approaching", await output);
            var report = XDocument.Load(Path.Combine(results.FullName, "result.trx"));
            XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
            var result = Assert.ContainsSingle(report.Descendants(trx + "UnitTestResult"));
            Assert.AreEqual("Passed", (string?)result.Attribute("outcome"));
            Assert.IsTrue((string?)result.Attribute("testName") is
                nameof(DeadlineTests.Imports_the_customers) or nameof(DeadlineTests.Imports_the_orders));
        }
        finally
        {
            results.Delete(true);
        }
    }
}
