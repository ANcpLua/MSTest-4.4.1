using System.Diagnostics;
using System.Reflection;
using MSTest.Showcase;

namespace MSTest.Platform.Fixed.MSBuild;

/// <summary>
/// One run of the MSBuild target <c>InvokeTestingPlatform</c> of this project through <c>dotnet msbuild</c>, the
/// target that <c>/t:Test</c> and the VSTest-based <c>dotnet test</c> run for a project on
/// Microsoft.Testing.Platform. The target
/// starts the built test application with the arguments of <c>TestingPlatformCommandLineArguments</c>, and
/// <c>TestingPlatformCaptureOutput=false</c> lets the application write to the output of MSBuild. The run
/// prints English, includes the <c>[Explicit]</c> demonstrations that its filter selects and has none of the
/// variables that a CI system adds to the environment of the machine.
/// </summary>
internal sealed record TestTargetRun(int ExitCode, string Output)
{
    private static readonly string[] Foreign =
        ["GITHUB_", "TF_BUILD", "SYSTEM_", "BUILD_", "AGENT_", "TESTINGPLATFORM_", "DOTNET_ROOT"];

    private static readonly string Project = typeof(TestTargetRun).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(static attribute => attribute.Key == "ProjectPath").Value!;

    public static async Task<TestTargetRun> StartAsync(
        string arguments,
        IReadOnlyDictionary<string, string> properties,
        IReadOnlyDictionary<string, string> environment,
        CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(
            "dotnet",
            [
                "msbuild", Project, "-t:InvokeTestingPlatform", "-nologo", "-v:minimal",
                "-p:TestingPlatformCaptureOutput=false",
                $"-p:TestingPlatformCommandLineArguments={arguments}",
                .. properties.Select(static property => $"-p:{property.Key}={property.Value}"),
            ])
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
        foreach (var (name, value) in environment)
        {
            start.Environment[name] = value;
        }

        await using var child = ChildProcess.Start(start, cancellationToken);
        var process = child.Process;
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return new TestTargetRun(process.ExitCode, output);
    }

    private static bool IsForeign(string variable) =>
        Foreign.Any(prefix => variable.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
