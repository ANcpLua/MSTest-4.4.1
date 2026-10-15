using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.Extension;

/// <summary>
///     The hook of an extension that a test application loads through a manifest. The platform looks for a
///     public static method <c>AddExtensions</c> that takes the builder and the arguments of the application and
///     returns nothing. It is the hook that a referenced extension has as well, so the extension registers here
///     what it would register there.
/// </summary>
// MTP discovers this type and AddExtensions by name from the dynamic-extension manifest.
// ReSharper disable UnusedType.Global, UnusedMember.Global
public static class TestingPlatformBuilderHook
{
    public static void AddExtensions(ITestApplicationBuilder builder, string[] _)
    {
        builder.TestHost.AddTestSessionLifetimeHandler(static services =>
            new ShiftLog(services.GetOutputDevice(), "Day shift"));
    }
}
// ReSharper restore UnusedType.Global, UnusedMember.Global
