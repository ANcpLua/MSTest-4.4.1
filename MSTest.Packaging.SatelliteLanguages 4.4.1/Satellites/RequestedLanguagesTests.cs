namespace MSTest.Packaging.SatelliteLanguages.Satellites;

/// <summary>
///     SatelliteResourceLanguages in the project file names the languages of the satellite assemblies to keep, here
///     fr;ja. The build copies MSTest.TestAdapter.resources.dll and MSTestAdapter.PlatformServices.resources.dll
///     into the output folder in these languages and in no other.
/// </summary>
[TestClass]
public sealed class RequestedLanguagesTests
{
    [TestMethod]
    [DataRow("MSTest.TestAdapter.resources.dll")]
    [DataRow("MSTestAdapter.PlatformServices.resources.dll")]
    public void Copies_the_satellite_assembly_in_the_requested_languages_only(string assembly)
    {
        string[] languages =
        [
            .. Directory.GetFiles(AppContext.BaseDirectory, assembly, SearchOption.AllDirectories)
                .Select(file => Path.GetRelativePath(AppContext.BaseDirectory, Path.GetDirectoryName(file)!))
                .Order(StringComparer.Ordinal)
        ];

        Assert.AreSequenceEqual(["fr", "ja"], languages);
    }
}