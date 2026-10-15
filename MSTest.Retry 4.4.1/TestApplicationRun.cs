using System.Diagnostics;
using System.Text.RegularExpressions;
using MSTest.Showcase;

namespace MSTest.Retry;

/// <summary>
/// One run of this test application in a process of its own, which lets an example assert what the test host
/// prints, which exit code it returns and which reports it writes. The run starts the process of the running
/// application again: its executable, or the <c>dotnet</c> host with the assembly when the application was
/// started through <c>dotnet exec</c>. It prints English and includes the <c>[Explicit]</c> demonstrations
/// that its filter selects.
/// </summary>
internal sealed record TestApplicationRun(int ExitCode, string Output)
{
    private const string Language = "TESTINGPLATFORM_UI_LANGUAGE";
    private const string Host = "dotnet";

    public static async Task<TestApplicationRun> StartAsync(CancellationToken cancellationToken, params string[] arguments)
    {
        var process = Environment.ProcessPath!;
        string[] assembly = Path.GetFileNameWithoutExtension(process) == Host
            ? ["exec", Environment.GetCommandLineArgs()[0]]
            : [];

        var start = new ProcessStartInfo(process, [.. assembly, .. arguments]) { RedirectStandardOutput = true };
        start.Environment[Language] = "en-us";
        start.Environment[ExplicitAttribute.Variable] = "1";

        await using var child = ChildProcess.Start(start, cancellationToken);
        var application = child.Process;
        var output = await application.StandardOutput.ReadToEndAsync(cancellationToken);
        await application.WaitForExitAsync(cancellationToken);

        return new TestApplicationRun(application.ExitCode, output);
    }

    // The result lines of one test without its name and duration, such as "failed (try 1)", in printed order.
    public IEnumerable<string> AttemptsOf(string test) =>
        Regex.Matches(Output, $@"^(\w+ \(try \d+\)) {Regex.Escape(test)} \(", RegexOptions.Multiline)
            .Select(static attempt => attempt.Groups[1].Value);
}
