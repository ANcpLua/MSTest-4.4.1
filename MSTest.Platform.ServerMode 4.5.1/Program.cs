using Microsoft.Testing.Platform.Builder;
using MSTest.Platform.ServerMode.Client;

namespace MSTest.Platform.ServerMode;

/// <summary>
/// The entry point of the test application, written by hand because <see cref="ClientReport"/> is registered
/// in code. <c>AddSelfRegisteredExtensions</c> registers MSTest and the extensions of the packages that the
/// project references, as the generated entry point does.
/// </summary>
internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = await TestApplication.CreateBuilderAsync(args);

        builder.AddSelfRegisteredExtensions(args);
        builder.TestHost.AddTestSessionLifetimeHandler(static services => new ClientReport(services));

        using var application = await builder.BuildAsync();

        return await application.RunAsync();
    }
}
