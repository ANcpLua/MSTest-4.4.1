using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Services;
using MSTest.Platform.Services;

namespace MSTest.Platform.Configuration;

/// <summary>
/// The sections merge the sources of the application, here the <c>testconfig.json</c> and
/// <see cref="CarrierDefaults"/>. The children of a section are the keys of all sources that implement
/// <see cref="IHierarchicalConfigurationProvider"/>, each key once and in alphabetical order. The value of a
/// section is the value of the first source that has one, in the order of the sources.
/// </summary>
[TestClass]
public sealed class ConfigurationSourceTests
{
    private static IConfigurationSection Carrier => TestHostServices.Current.GetConfigurationRoot().GetSection("showcase:carrier");

    [TestMethod]
    public void Children_are_the_keys_of_the_file_and_of_the_defaults() =>
        Assert.AreSequenceEqual(["name", "pickup"], Carrier.GetChildren().Select(static child => child.Key));

    [TestMethod]
    public void Value_of_the_file_replaces_the_default() =>
        Assert.AreEqual("Seapost", Carrier.GetSection("name").Value);

    [TestMethod]
    public void Default_is_the_value_where_the_file_has_none() =>
        Assert.AreEqual("17:00", Carrier.GetSection("pickup").Value);

    [TestMethod]
    public void Top_level_section_lists_the_sections_of_both_sources() =>
        Assert.AreSequenceEqual(
            ["carrier", "reports"],
            TestHostServices.Current.GetConfigurationRoot().GetSection("showcase").GetChildren().Select(static child => child.Key));
}
