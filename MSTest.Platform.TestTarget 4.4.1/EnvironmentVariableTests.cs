namespace MSTest.Platform.TestTarget;

/// <summary>
/// A <c>TestingPlatformEnvironmentVariable</c> item of the project file sets an environment variable of the
/// test application that the MSBuild target <c>Test</c> starts: <c>Include</c> is the name and <c>Value</c>
/// the value, which keeps its semicolons. The items are added to the environment that the build has, and they
/// apply to that target only. <c>dotnet build -t:Test</c> runs the target, and an application that the target
/// starts has the option <c>--internal-msbuild-node</c> on its command line. An application that is started
/// in another way, by <c>dotnet run</c> for one, does not get the variables.
/// </summary>
[TestClass]
public sealed class EnvironmentVariableTests
{
    private static readonly bool StartedByTheTestTarget = Environment.GetCommandLineArgs().Contains("--internal-msbuild-node");

    [TestMethod]
    public void Item_sets_the_variable_of_an_application_that_the_target_starts() =>
        Assert.AreEqual(StartedByTheTestTarget ? "eu-central" : null, Environment.GetEnvironmentVariable("SHOWCASE_REGION"));

    [TestMethod]
    public void Value_keeps_its_semicolon() =>
        Assert.AreEqual(
            StartedByTheTestTarget ? "https://a.example/index.json;https://b.example/index.json" : null,
            Environment.GetEnvironmentVariable("SHOWCASE_FEEDS"));

    [TestMethod]
    public void Environment_of_the_build_stays() =>
        Assert.IsNotNull(Environment.GetEnvironmentVariable("PATH"));
}
