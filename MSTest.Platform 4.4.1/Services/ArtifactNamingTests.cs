using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.Services;

/// <summary>
/// <see cref="IArtifactNamingService"/> turns a template into the name of a file that an extension writes.
/// <c>GetArtifactNamingService</c> gets it from the services of the platform. <c>ResolveFileName</c> replaces
/// <c>{asm}</c>, <c>{tfm}</c>, <c>{arch}</c>, <c>{pname}</c>, <c>{pid}</c> and <c>{time}</c> and leaves any other
/// placeholder as it is. It keeps the directory of the template and replaces characters of the file name
/// such as a space, a colon or a bracket by an underscore.
/// </summary>
[TestClass]
public sealed class ArtifactNamingTests
{
    private static readonly string Architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

    private static IArtifactNamingService Names => TestHostServices.Current.GetArtifactNamingService();

    [TestMethod]
    public void Assembly_framework_and_architecture_are_those_of_the_test_application() =>
        Assert.AreEqual($"MSTest.Platform_4.4.1-net10.0-{Architecture}.log", Names.ResolveFileName("{asm}-{tfm}-{arch}.log"));

    [TestMethod]
    public void Process_is_named_by_the_file_of_the_test_application_and_by_its_id() =>
        Assert.AreEqual($"MSTest.Platform_4.4.1-{Environment.ProcessId}.dmp", Names.ResolveFileName("{pname}-{pid}.dmp"));

    [TestMethod]
    public void Time_is_the_current_time_in_utc_down_to_the_tick()
    {
        var before = DateTimeOffset.UtcNow;

        var name = Names.ResolveFileName("{time}");

        var time = DateTimeOffset.ParseExact(name, "yyyy-MM-dd_HH-mm-ss.fffffff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        Assert.IsInRange(before, DateTimeOffset.UtcNow, time);
    }

    [TestMethod]
    public void Unknown_placeholder_stays_as_it_is() =>
        Assert.AreEqual("{run}-{ASM}.log", Names.ResolveFileName("{run}-{ASM}.log"));

    [TestMethod]
    public void Spaces_colons_and_brackets_of_the_file_name_become_underscores() =>
        Assert.AreEqual("stock_count__aisle_7_.log", Names.ResolveFileName("stock count (aisle:7).log"));

    [TestMethod]
    public void Directory_of_the_template_is_kept_as_it_is() =>
        Assert.AreEqual(
            Path.Combine("stock count", $"{Architecture}_log.txt"),
            Names.ResolveFileName(Path.Combine("stock count", "{arch} log.txt")));

    [TestMethod]
    public void Template_that_names_no_file_is_rejected() =>
        Assert.ThrowsExactly<ArgumentException>(() => Names.ResolveFileName($"logs{Path.DirectorySeparatorChar}"));
}
