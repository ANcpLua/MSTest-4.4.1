namespace MSTest.Packaging.Satellites;

/// <summary>
/// A Microsoft.Testing.Platform project (EnableMSTestRunner, which MSTest.Sdk sets) without
/// SatelliteResourceLanguages gets the satellite assemblies of the adapter in every language of the
/// MSTest.TestAdapter package, whatever the culture of the build machine. The build copies
/// MSTest.TestAdapter.resources.dll and MSTestAdapter.PlatformServices.resources.dll into the output folder, one
/// folder per language.
/// </summary>
[TestClass]
public sealed class EveryLanguageTests
{
    [TestMethod]
    [DataRow("MSTest.TestAdapter.resources.dll")]
    [DataRow("MSTestAdapter.PlatformServices.resources.dll")]
    public void Copies_the_satellite_assembly_in_every_language(string assembly)
    {
        string[] languages =
        [
            .. Directory.GetFiles(AppContext.BaseDirectory, assembly, SearchOption.AllDirectories)
                .Select(file => Path.GetRelativePath(AppContext.BaseDirectory, Path.GetDirectoryName(file)!))
                .Order(StringComparer.Ordinal),
        ];

        Assert.AreSequenceEqual(
            ["cs", "de", "es", "fr", "it", "ja", "ko", "pl", "pt-BR", "ru", "tr", "zh-Hans", "zh-Hant"],
            languages);
    }
}
