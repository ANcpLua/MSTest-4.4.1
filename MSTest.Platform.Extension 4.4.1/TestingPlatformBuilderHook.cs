using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.Extension;

/// <summary>
/// The hook of an extension that a test application loads through a manifest. The platform looks for a
/// public static method <c>AddExtensions</c> that takes the builder and the arguments of the application and
/// returns nothing. It is the hook that a referenced extension has as well, so the extension registers here
/// what it would register there.
/// </summary>
public static class TestingPlatformBuilderHook
{
    public static void AddExtensions(ITestApplicationBuilder builder, string[] args) =>
        builder.TestHost.AddTestSessionLifetimeHandler(static services => new ShiftLog(services.GetOutputDevice(), "Day shift"));
}
