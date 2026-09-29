using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Testing.Platform.ServerMode.Client;

namespace MSTest.Platform.Fixed.ServerMode;

/// <summary>
/// <c>MtpServerClient.LaunchAsync</c> of the package <c>Microsoft.Testing.Platform.ServerMode.Client.Sources</c>
/// takes the assembly of a test application and starts its apphost, the executable next to it. On macOS and
/// Linux an apphost that lost its execute permission, as a file does that travels through a zip archive, is
/// passed over, and the client starts the assembly with <c>dotnet</c> instead. Here the client starts a copy of
/// the output folder of this test application, once as it is and once after the example removes the execute
/// permission of the apphost. The class runs on macOS and Linux only.
/// </summary>
[TestClass]
[OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
[UnsupportedOSPlatform("windows")]
public sealed class AppHostFallbackTests(TestContext testContext)
{
    private static DirectoryInfo _copy = null!;
    private static string _assembly = null!;
    private static string _apphost = null!;

    [ClassInitialize]
    public static void CopyTheTestApplication(TestContext testContext)
    {
        _copy = Directory.CreateTempSubdirectory();
        foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(_copy.FullName, Path.GetRelativePath(AppContext.BaseDirectory, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }

        _assembly = Path.Combine(_copy.FullName, Path.GetFileName(typeof(AppHostFallbackTests).Assembly.Location));
        _apphost = Path.Combine(_copy.FullName, Path.GetFileName(Environment.ProcessPath)!);
    }

    [ClassCleanup]
    public static void DeleteTheCopy() => _copy.Delete(recursive: true);

    [TestMethod]
    [DoNotParallelize]
    public async Task Client_starts_the_apphost_when_it_can_be_executed()
    {
        using var client = await MtpServerClient.LaunchAsync(_assembly, cancellationToken: testContext.CancellationToken);
        var capabilities = await client.InitializeAsync(testContext.CancellationToken);

        Assert.AreNotEqual("dotnet", Process.GetProcessById(capabilities.ServerProcessId!.Value).ProcessName);

        await client.ExitAsync(testContext.CancellationToken);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task Client_starts_dotnet_when_the_apphost_cannot_be_executed()
    {
        var mode = File.GetUnixFileMode(_apphost);
        File.SetUnixFileMode(_apphost, mode & ~(UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute));
        try
        {
            using var client = await MtpServerClient.LaunchAsync(_assembly, cancellationToken: testContext.CancellationToken);
            var capabilities = await client.InitializeAsync(testContext.CancellationToken);

            Assert.AreEqual("dotnet", Process.GetProcessById(capabilities.ServerProcessId!.Value).ProcessName);

            await client.ExitAsync(testContext.CancellationToken);
        }
        finally
        {
            File.SetUnixFileMode(_apphost, mode);
        }
    }
}
