using Microsoft.Testing.Platform.Builder;
using MSTest.Packaging.AssemblyResolution.ResolverLogging;

namespace MSTest.Packaging.AssemblyResolution;

/// <summary>
/// The entry point of the test application, written by hand to register a logger provider next to MSTest.
/// </summary>
internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = await TestApplication.CreateBuilderAsync(args);

        builder.AddSelfRegisteredExtensions(args);
        builder.Logging.AddProvider(static (_, _) => new FailingResolverLoggerProvider());

        using var application = await builder.BuildAsync();

        return await application.RunAsync();
    }
}
