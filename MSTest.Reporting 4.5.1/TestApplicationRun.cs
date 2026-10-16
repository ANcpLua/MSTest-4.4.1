using System.Diagnostics;
using System.Xml.Linq;
using MSTest.Reporting.Spans;
using MSTest.Showcase;

namespace MSTest.Reporting;

/// <summary>
///     One run of this test application in a process of its own, which lets an example assert what the test host
///     prints, which exit code it returns and which report and spans it writes. The run prints English, includes
///     the <c>[Explicit]</c> demonstrations that its filter selects and has a results directory of its own.
/// </summary>
internal sealed record TestApplicationRun(int ExitCode, string Output, DirectoryInfo Results) : IDisposable
{
    private const string Language = "TESTINGPLATFORM_UI_LANGUAGE";
    private const string SpanFile = "spans.jsonl";

    public void Dispose()
    {
        Results.Delete(true);
    }

    public static async Task<TestApplicationRun> StartAsync(CancellationToken cancellationToken,
        params string[] arguments)
    {
        var results = Directory.CreateTempSubdirectory();
        var start = new ProcessStartInfo(Environment.ProcessPath!,
            [.. arguments, "--results-directory", results.FullName])
        {
            RedirectStandardOutput = true,
            Environment =
            {
                [Language] = "en-us",
                [ExplicitAttribute.Variable] = "1",
                [SpanFileExporter.Variable] = Path.Combine(results.FullName, SpanFile)
            }
        };

        await using var child = ChildProcess.Start(start, cancellationToken);
        var application = child.Process;
        var output = await application.StandardOutput.ReadToEndAsync(cancellationToken);
        await application.WaitForExitAsync(cancellationToken);

        return new TestApplicationRun(application.ExitCode, output, results);
    }

    public IReadOnlyList<Span> ReadSpans()
    {
        return [.. File.ReadLines(Path.Combine(Results.FullName, SpanFile)).Select(Span.Parse)];
    }

    public XDocument ReadReport(string file)
    {
        return XDocument.Load(Path.Combine(Results.FullName, file));
    }
}