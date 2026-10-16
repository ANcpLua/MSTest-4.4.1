using System.Diagnostics.CodeAnalysis;

namespace MSTest.NativeAot;

/// <summary>
/// Tells the two hosts of this project apart. <c>dotnet run</c> starts the managed assembly, and
/// <c>dotnet publish</c> compiles it into a native executable, where an assembly has no location.
/// </summary>
internal static class NativeExecutable
{
    public static bool IsRunning
    {
        [UnconditionalSuppressMessage("SingleFile", "IL3000", Justification = "The empty location is what tells the native executable apart.")]
        get => typeof(NativeExecutable).Assembly.Location.Length == 0;
    }
}
