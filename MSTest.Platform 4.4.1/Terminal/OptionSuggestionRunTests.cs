namespace MSTest.Platform.Terminal;

/// <summary>
/// What the test application prints for an option that it does not know. It suggests the option whose name
/// is closest to the unknown one, a few letters apart at most, and makes no suggestion when two options are
/// equally close. For an option of an extension that the project does not reference, it names the package
/// that provides the option. This project references no <c>Microsoft.Testing.Extensions.CrashDump</c>, so
/// <c>--crashdump</c> is such an option. Every such run ends with exit code 5 and runs no test.
/// </summary>
[TestClass]
public sealed class OptionSuggestionRunTests(TestContext testContext)
{
    private const string CrashDumpPackage =
        "Option '--crashdump' is provided by the 'Microsoft.Testing.Extensions.CrashDump' extension. Add a package reference to use it.";

    private const int InvalidCommandLine = 5;

    [TestMethod]
    public async Task Misspelled_option_gets_the_closest_option_as_a_suggestion()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, "--show-slowest-test", "3");

        Assert.AreSequenceEqual(
            ["Unknown option '--show-slowest-test'", "Did you mean '--show-slowest-tests'?"],
            FirstLinesOf(run, 2));
        Assert.AreEqual(InvalidCommandLine, run.ExitCode);
    }

    [TestMethod]
    public async Task Option_between_two_equally_close_options_gets_no_suggestion()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, "--show-stdor", "all");

        Assert.AreEqual("Unknown option '--show-stdor'", FirstLinesOf(run, 1)[0]);
        Assert.DoesNotContain("Did you mean", run.Output);
        Assert.AreEqual(InvalidCommandLine, run.ExitCode);
    }

    [TestMethod]
    public async Task Option_of_a_missing_extension_names_the_package_that_provides_it()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, "--crashdump");

        Assert.AreSequenceEqual(["Unknown option '--crashdump'", CrashDumpPackage], FirstLinesOf(run, 2));
        Assert.AreEqual(InvalidCommandLine, run.ExitCode);
    }

    [TestMethod]
    public async Task Misspelled_option_of_a_missing_extension_gets_the_suggestion_and_the_package()
    {
        using var run = await TestApplicationRun.StartAsync(testContext.CancellationToken, "--crashdmp");

        Assert.AreSequenceEqual(
            ["Unknown option '--crashdmp'", "Did you mean '--crashdump'?", CrashDumpPackage],
            FirstLinesOf(run, 3));
        Assert.AreEqual(InvalidCommandLine, run.ExitCode);
    }

    private static string[] FirstLinesOf(TestApplicationRun run, int count) =>
        [.. run.Output.ReplaceLineEndings("\n").Split('\n').Take(count)];
}
