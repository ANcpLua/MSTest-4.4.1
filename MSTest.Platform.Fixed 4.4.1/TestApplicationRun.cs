using System.Diagnostics;
using MSTest.Showcase;

namespace MSTest.Platform.Fixed;

/// <summary>
/// One run of a test application in a process of its own, which lets an example assert what the application
/// prints on its output and its error stream, which exit code it returns and which files it writes. The run
/// prints English, sends no telemetry and includes the <c>[Explicit]</c> demonstrations that its filter
/// selects. It has the variables that the example passes and none of those that a CI system or a platform
/// setting adds to the environment of the machine. An example can act on the process while it runs, given its
/// process id.
/// </summary>
internal sealed record TestApplicationRun(int ExitCode, string Output, string Error, DirectoryInfo Results) : IDisposable
{
    private static readonly string[] Foreign =
        ["GITHUB_", "TF_BUILD", "SYSTEM_", "BUILD_", "AGENT_", "TESTINGPLATFORM_", "DOTNET_WATCH"];

    public static Task<TestApplicationRun> StartAsync(CancellationToken cancellationToken, params string[] arguments) =>
        StartAsync(new Dictionary<string, string>(), cancellationToken, arguments);

    public static Task<TestApplicationRun> StartAsync(
        IReadOnlyDictionary<string, string> environment,
        CancellationToken cancellationToken,
        params string[] arguments) =>
        StartAsync(Environment.ProcessPath!, Directory.CreateTempSubdirectory(), environment, cancellationToken, arguments);

    public static Task<TestApplicationRun> StartAsync(
        string application,
        DirectoryInfo results,
        IReadOnlyDictionary<string, string> environment,
        CancellationToken cancellationToken,
        params string[] arguments) =>
        StartAsync(application, results, environment, static _ => Task.CompletedTask, cancellationToken, arguments);

    public static async Task<TestApplicationRun> StartAsync(
        string application,
        DirectoryInfo results,
        IReadOnlyDictionary<string, string> environment,
        Func<int, Task> whileRunning,
        CancellationToken cancellationToken,
        params string[] arguments)
    {
        var start = new ProcessStartInfo(application, [.. arguments, "--results-directory", results.FullName])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var name in start.Environment.Keys.Where(IsForeign).ToArray())
        {
            start.Environment.Remove(name);
        }

        start.Environment["TESTINGPLATFORM_UI_LANGUAGE"] = "en-us";
        start.Environment["TESTINGPLATFORM_TELEMETRY_OPTOUT"] = "1";
        start.Environment[ExplicitAttribute.Variable] = "1";
        foreach (var (name, value) in environment)
        {
            start.Environment[name] = value;
        }

        using var process = Process.Start(start)!;
        await using var cancellation = cancellationToken.Register(() => process.Kill(entireProcessTree: true));
        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = process.StandardError.ReadToEndAsync(cancellationToken);
        await whileRunning(process.Id);
        await process.WaitForExitAsync(cancellationToken);

        return new TestApplicationRun(process.ExitCode, await output, await error, results);
    }

    public string PathOf(string file) => Path.Combine(Results.FullName, file);

    public void Dispose() => Results.Delete(recursive: true);

    private static bool IsForeign(string variable) =>
        Foreign.Any(prefix => variable.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
