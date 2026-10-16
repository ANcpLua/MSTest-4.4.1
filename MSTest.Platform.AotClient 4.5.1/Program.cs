using System.Diagnostics.CodeAnalysis;
using Microsoft.Testing.Platform.ServerMode.Client;

namespace MSTest.Platform.AotClient;

/// <summary>
/// A client of the server mode that <c>dotnet publish</c> compiles into a native executable. The project
/// references <c>Microsoft.Testing.Platform.ServerMode.Client.Sources</c> and no other package, sets
/// <c>PublishAot</c> and treats every warning as an error, so a publish without a warning shows that the
/// client raises no trimming or Native AOT warning. The executable starts the test application that its
/// first argument names in server mode, discovers its tests, runs the test that its second argument names and
/// prints its outcome. It returns 0 when the test passed and 1 otherwise.
/// </summary>
internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var client = await MtpServerClient.LaunchAsync(Path.GetFullPath(args[0]));
        var nodes = new List<MtpTestNodeUpdate>();
        client.TestNodesUpdated += (_, update) => nodes.AddRange(update.Changes);
        var capabilities = await client.InitializeAsync();
        await client.DiscoverTestsAsync();
        var discovered = nodes.Where(static node => node.NodeType == "action").ToList();
        nodes.Clear();

        await client.RunTestsAsync([discovered.Single(node => node.DisplayName == args[1]).Uid!]);
        await client.ExitAsync();

        Console.WriteLine($"Native executable: {IsNativeExecutable()}");
        Console.WriteLine($"Server {capabilities.ServerVersion}, {discovered.Count} tests discovered");
        Console.WriteLine($"{nodes[^1].ExecutionState} {nodes[^1].DisplayName}");

        return nodes[^1].ExecutionState == "passed" ? 0 : 1;
    }

    // Under dotnet run the assembly is a file; in the native executable it has no location.
    [UnconditionalSuppressMessage("SingleFile", "IL3000", Justification = "The empty location is what tells the native executable apart.")]
    private static bool IsNativeExecutable() => typeof(Program).Assembly.Location.Length == 0;
}
