using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.Extension;

/// <summary>
///     A second hook of the same assembly, which a manifest names by its type as it names the first one.
/// </summary>
// MTP discovers this type and AddExtensions by name from the dynamic-extension manifest.
// ReSharper disable UnusedType.Global, UnusedMember.Global
public static class NightShiftHook
{
    public static void AddExtensions(ITestApplicationBuilder builder, string[] _)
    {
        builder.TestHost.AddTestSessionLifetimeHandler(static services =>
            new ShiftLog(services.GetOutputDevice(), "Night shift"));
    }
}
// ReSharper restore UnusedType.Global, UnusedMember.Global
