using System.Diagnostics;

namespace MSTest.SourceGeneration;

/// <summary>
/// One run of this test application in a process of its own, started through the executable of the running
/// application. It lets an example assert what the test host prints and which exit code it returns. The run
/// prints English.
/// </summary>
internal sealed record TestApplicationRun(int ExitCode, string Output)
{
    private const string Language = "TESTINGPLATFORM_UI_LANGUAGE";

    public static async Task<TestApplicationRun> StartAsync(CancellationToken cancellationToken, params string[] arguments)
    {
        var start = new ProcessStartInfo(Environment.ProcessPath!, arguments) { RedirectStandardOutput = true };
        start.Environment[Language] = "en-us";

        using var application = Process.Start(start)!;
        var output = await application.StandardOutput.ReadToEndAsync(cancellationToken);
        await application.WaitForExitAsync(cancellationToken);

        return new TestApplicationRun(application.ExitCode, output);
    }
}
