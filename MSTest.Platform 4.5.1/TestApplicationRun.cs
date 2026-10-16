using System.Diagnostics;
using System.Text.RegularExpressions;
using MSTest.Showcase;

namespace MSTest.Platform;

/// <summary>
///     One run of this test application in a process of its own, which lets an example assert what the test host
///     prints, which exit code it returns and which files it writes. The run prints English and no banner,
///     includes the <c>[Explicit]</c> demonstrations that its filter selects and writes to a results directory of
///     its own.
/// </summary>
internal sealed partial record TestApplicationRun(int ExitCode, string Output, string Error, DirectoryInfo Results)
    : IDisposable
{
    private const string Language = "TESTINGPLATFORM_UI_LANGUAGE";
    private const string NoBanner = "TESTINGPLATFORM_NOBANNER";

    // The result lines of the tests without their duration, such as "passed Ships_a_paid_order", as printed.
    public IReadOnlyList<string> ResultLines =>
        [.. ResultLine().Matches(Output).Select(static line => line.Groups[1].Value)];

    public void Dispose()
    {
        Results.Delete(true);
    }

    public static Task<TestApplicationRun> StartAsync(CancellationToken cancellationToken, params string[] arguments)
    {
        return StartAsync(new Dictionary<string, string?>(), cancellationToken, arguments);
    }

    public static Task<TestApplicationRun> StartAsync(
        IReadOnlyDictionary<string, string?> environment,
        CancellationToken cancellationToken,
        params string[] arguments)
    {
        return StartAsync(Directory.CreateTempSubdirectory(), environment, cancellationToken, arguments);
    }

    // A results directory that exists before the run, for a variable that names a file in it.
    public static Task<TestApplicationRun> StartAsync(
        DirectoryInfo results,
        IReadOnlyDictionary<string, string?> environment,
        CancellationToken cancellationToken,
        params string[] arguments)
    {
        return LaunchAsync(results, environment, [.. arguments, "--results-directory", results.FullName],
            cancellationToken);
    }

    // The indented lines that the terminal prints below a heading, without their indentation.
    public IReadOnlyList<string> LinesBelow(string heading)
    {
        return
        [
            .. Output.ReplaceLineEndings("\n").Split('\n')
                .SkipWhile(line => !line.TrimStart().StartsWith(heading))
                .Skip(1)
                .TakeWhile(static line => line.StartsWith(' '))
                .Select(static line => line.Trim())
        ];
    }

    public string PathOf(string file)
    {
        return Path.Combine(Results.FullName, file);
    }

    private static async Task<TestApplicationRun> LaunchAsync(
        DirectoryInfo results,
        IReadOnlyDictionary<string, string?> environment,
        string[] arguments,
        CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(Environment.ProcessPath!, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Environment =
            {
                [Language] = "en-us",
                [NoBanner] = "1",
                [ExplicitAttribute.Variable] = "1"
            }
        };

        foreach (var (name, value) in environment) start.Environment[name] = value;

        await using var child = ChildProcess.Start(start, cancellationToken);
        var application = child.Process;
        var error = application.StandardError.ReadToEndAsync(cancellationToken);
        var output = await application.StandardOutput.ReadToEndAsync(cancellationToken);
        await application.WaitForExitAsync(cancellationToken);

        return new TestApplicationRun(application.ExitCode, output, await error, results);
    }

    [GeneratedRegex(@"^((?:passed|failed|skipped)(?: \(try \d+\))? \S+) \(\d[^)]*\)\r?$", RegexOptions.Multiline)]
    private static partial Regex ResultLine();
}