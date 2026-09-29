using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Services;
using MSTest.Platform.Services;

namespace MSTest.Platform.Configuration;

/// <summary>
/// <see cref="IConfigurationRoot"/> is the configuration of the platform as a tree, which
/// <c>GetConfigurationRoot</c> gets from the services of the platform. <c>GetSection</c> takes a path with
/// <c>:</c> between its keys and returns an <see cref="IConfigurationSection"/> with <c>Key</c>,
/// <c>Path</c>, <c>HasValue</c> and <c>Value</c>, and <c>GetChildren</c> returns the sections directly below.
/// The values come from the <c>showcase</c> object in the <c>testconfig.json</c> of this project. The item of
/// an array is a section whose key is its index.
/// </summary>
[TestClass]
public sealed class ConfigurationSectionTests
{
    private static IConfigurationRoot Configuration => TestHostServices.Current.GetConfigurationRoot();

    [TestMethod]
    public void Root_has_the_sections_of_the_file_and_of_the_platform()
    {
        var sections = Configuration.GetChildren().Select(static section => section.Key).ToArray();

        Assert.Contains("showcase", sections);
        Assert.Contains("platformOptions", sections);
    }

    [TestMethod]
    public void Section_of_a_value_has_the_last_key_the_whole_path_and_the_value()
    {
        var keep = Configuration.GetSection("showcase:reports:keep");

        Assert.AreEqual("keep", keep.Key);
        Assert.AreEqual("showcase:reports:keep", keep.Path);
        Assert.IsTrue(keep.HasValue);
        Assert.AreEqual("30", keep.Value);
    }

    [TestMethod]
    public void Section_of_an_object_has_children_and_no_value()
    {
        var reports = Configuration.GetSection("showcase:reports");

        Assert.IsFalse(reports.HasValue);
        Assert.IsNull(reports.Value);
        Assert.AreSequenceEqual(["formats", "keep"], reports.GetChildren().Select(static child => child.Key));
    }

    [TestMethod]
    public void Items_of_an_array_are_children_named_by_their_index()
    {
        var formats = Configuration.GetSection("showcase:reports:formats").GetChildren().ToArray();

        Assert.AreSequenceEqual(["0", "1"], formats.Select(static format => format.Key));
        Assert.AreSequenceEqual(["trx", "ctrf"], formats.Select(static format => format.Value));
        Assert.AreEqual("showcase:reports:formats:1", formats[1].Path);
    }

    [TestMethod]
    public void Section_is_the_root_of_the_paths_below_it()
    {
        var reports = Configuration.GetSection("showcase").GetSection("reports");

        Assert.AreEqual("showcase:reports:keep", reports.GetSection("keep").Path);
        Assert.AreEqual("30", reports["keep"]);
        Assert.AreEqual("ctrf", reports["formats:1"]);
    }

    [TestMethod]
    public void Path_is_not_case_sensitive() =>
        Assert.AreEqual("30", Configuration.GetSection("SHOWCASE:Reports:KEEP").Value);

    [TestMethod]
    public void Section_that_no_source_knows_is_returned_empty()
    {
        var invoices = Configuration.GetSection("showcase:invoices");

        Assert.AreEqual("invoices", invoices.Key);
        Assert.IsFalse(invoices.HasValue);
        Assert.IsEmpty(invoices.GetChildren());
    }
}
