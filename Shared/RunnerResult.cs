using System.Diagnostics;
using System.Xml.Linq;

namespace MSTest.Showcase;

internal sealed record RunnerResult(int ExitCode, string Output, XDocument Report)
{
    public IReadOnlyList<Result> Tests
    {
        get
        {
            XNamespace trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
            var classes = Report.Descendants(trx + "UnitTest").ToDictionary(
                test => (string)test.Attribute("id")!,
                test => (string)test.Element(trx + "TestMethod")!.Attribute("className")!);
            return [.. Report.Descendants(trx + "UnitTestResult")
                .Select(result => new Result(classes[(string)result.Attribute("testId")!],
                    (string)result.Attribute("testName")!, (string)result.Attribute("outcome")!))];
        }
    }

    public sealed record Result(string ClassName, string Name, string Outcome);

    public static async Task<RunnerResult> RunAsync(string filter, CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string?>? environment = null)
    {
        var results = Directory.CreateTempSubdirectory();
        try
        {
            ProcessStartInfo start = new(Environment.ProcessPath!, ["--filter", filter,
                "--report-trx", "--report-trx-filename", "result.trx", "--results-directory", results.FullName])
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment = { [ExplicitAttribute.Variable] = "1", ["TESTINGPLATFORM_UI_LANGUAGE"] = "en-us" }
            };
            if (environment is not null)
                foreach (var (key, value) in environment) start.Environment[key] = value;
            await using var child = ChildProcess.Start(start, cancellationToken);
            var output = child.Process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = child.Process.StandardError.ReadToEndAsync(cancellationToken);
            await child.Process.WaitForExitAsync(cancellationToken);
            return new RunnerResult(child.Process.ExitCode, await output + await error,
                XDocument.Load(Path.Combine(results.FullName, "result.trx")));
        }
        finally
        {
            results.Delete(true);
        }
    }
}
