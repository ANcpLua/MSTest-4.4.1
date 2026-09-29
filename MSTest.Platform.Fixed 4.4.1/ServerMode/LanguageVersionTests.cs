using System.Reflection;

namespace MSTest.Platform.Fixed.ServerMode;

/// <summary>
/// The package <c>Microsoft.Testing.Platform.ServerMode.Client.Sources</c> adds source files to the project that
/// references it and leaves the C# version of the project at the default of its target framework, C# 14 for
/// net10.0. This project references the package, keeps its <c>LangVersion</c> as assembly metadata and uses
/// the <c>field</c> keyword of C# 14 in a property of this class.
/// </summary>
[TestClass]
public sealed class LanguageVersionTests
{
    public string Shelf { get => field; set => field = value.Trim(); } = string.Empty;

    [TestMethod]
    public void Project_compiles_with_the_default_language_version_of_net10() =>
        Assert.AreEqual(
            "14.0",
            typeof(LanguageVersionTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(static attribute => attribute.Key == "LangVersion").Value);

    [TestMethod]
    public void Property_with_the_field_keyword_keeps_the_trimmed_value()
    {
        Shelf = "  A-12  ";

        Assert.AreEqual("A-12", Shelf);
    }
}
