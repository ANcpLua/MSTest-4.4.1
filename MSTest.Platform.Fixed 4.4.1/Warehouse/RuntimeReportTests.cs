using System.Runtime.InteropServices;
using MSTest.Showcase;

namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>
///     A test that writes the variables with which the .NET host of its process looked for the runtime, and the
///     <c>DOTNET_HOST_PATH</c> that MSBuild passes to the processes it starts, into the file that the variable
///     <c>MSTEST_RUNTIME_REPORT</c> names, one <c>NAME=value</c> line each. Expected outcome with MSTEST_EXPLICIT=1,
///     MSTEST_RUNTIME_REPORT set to a file path and a filter on this class: the test passes and the file holds the
///     lines of <c>DOTNET_HOST_PATH</c>, <c>DOTNET_ROOT</c> and <c>DOTNET_ROOT_</c> with the architecture of the
///     process, <c>DOTNET_ROOT_ARM64</c> on this machine.
/// </summary>
[TestClass]
[Explicit]
public sealed class RuntimeReportTests
{
    public const string Report = "MSTEST_RUNTIME_REPORT";

    public static readonly string ArchitectureRoot =
        $"DOTNET_ROOT_{RuntimeInformation.ProcessArchitecture.ToString().ToUpperInvariant()}";

    [TestMethod]
    public void Reports_where_the_host_looked_for_the_runtime()
    {
        var report = Environment.GetEnvironmentVariable(Report)!;

        File.WriteAllLines(
            report,
            [
                $"DOTNET_HOST_PATH={Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")}",
                $"DOTNET_ROOT={Environment.GetEnvironmentVariable("DOTNET_ROOT")}",
                $"{ArchitectureRoot}={Environment.GetEnvironmentVariable(ArchitectureRoot)}"
            ]);

        Assert.IsTrue(File.Exists(report));
    }
}