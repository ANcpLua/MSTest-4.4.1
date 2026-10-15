using System.Diagnostics;
using MSTest.Platform.Changed.Telemetry;
using MSTest.Showcase;

namespace MSTest.Platform.Changed;

/// <summary>
/// One run of this test application in a process of its own, which lets an example assert what the test host
/// prints, which exit code it returns and which files it writes. The run prints English, sends no telemetry,
/// includes the <c>[Explicit]</c> demonstrations that its filter selects and writes its spans and instruments
/// into its results directory. It has the variables that the example passes and none of those that a CI
/// system or an OpenTelemetry setup adds to the environment of the machine.
/// </summary>
internal sealed record TestApplicationRun(int ExitCode, string Output, DirectoryInfo Results) : IDisposable
{
    private const string SpanFile = "spans.jsonl";
    private const string InstrumentFile = "instruments.jsonl";

    private static readonly string[] Foreign =
        ["GITHUB_", "TF_BUILD", "GITLAB_CI", "JENKINS_URL", "OTEL_", "TESTINGPLATFORM_OTEL_", "TRACEPARENT", "TRACESTATE"];

    public static Task<TestApplicationRun> StartAsync(CancellationToken cancellationToken, params string[] arguments) =>
        StartAsync(Directory.CreateTempSubdirectory(), new Dictionary<string, string>(), cancellationToken, arguments);

    public static Task<TestApplicationRun> StartAsync(
        IReadOnlyDictionary<string, string> environment,
        CancellationToken cancellationToken,
        params string[] arguments) =>
        StartAsync(Directory.CreateTempSubdirectory(), environment, cancellationToken, arguments);

    public static async Task<TestApplicationRun> StartAsync(
        DirectoryInfo results,
        IReadOnlyDictionary<string, string> environment,
        CancellationToken cancellationToken,
        params string[] arguments)
    {
        var start = new ProcessStartInfo(Environment.ProcessPath!, [.. arguments, "--results-directory", results.FullName])
        {
            RedirectStandardOutput = true,
        };
        foreach (var name in start.Environment.Keys.Where(IsForeign).ToArray())
        {
            start.Environment.Remove(name);
        }

        start.Environment["TESTINGPLATFORM_UI_LANGUAGE"] = "en-us";
        start.Environment["TESTINGPLATFORM_TELEMETRY_OPTOUT"] = "1";
        start.Environment[ExplicitAttribute.Variable] = "1";
        start.Environment[SpanFileExporter.Variable] = Path.Combine(results.FullName, SpanFile);
        start.Environment[InstrumentFileExporter.Variable] = Path.Combine(results.FullName, InstrumentFile);
        foreach (var (name, value) in environment)
        {
            start.Environment[name] = value;
        }

        await using var child = ChildProcess.Start(start, cancellationToken);
        var application = child.Process;
        var output = await application.StandardOutput.ReadToEndAsync(cancellationToken);
        await application.WaitForExitAsync(cancellationToken);

        return new TestApplicationRun(application.ExitCode, output, results);
    }

    public string PathOf(string file) => Path.Combine(Results.FullName, file);

    public bool HasSpans => File.Exists(PathOf(SpanFile));

    public IReadOnlyList<Span> ReadSpans() => [.. File.ReadLines(PathOf(SpanFile)).Select(Span.Parse)];

    public IReadOnlyList<Instrument> ReadInstruments() => [.. File.ReadLines(PathOf(InstrumentFile)).Select(Instrument.Parse)];

    public void Dispose() => Results.Delete(recursive: true);

    private static bool IsForeign(string variable) =>
        Foreign.Any(prefix => variable.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
