using System.Globalization;
using System.Text.RegularExpressions;

namespace MSTest.Platform.Fixed.Reports;

/// <summary>
/// The summary of the HTML report states the time from the start to the end of the run, not the sum of the
/// durations of the tests, so a run whose tests run in parallel reports how long it took. The page keeps the
/// summary as <c>totalDurationMs</c> and each test as <c>durationMs</c> in the data of the report. Here a run
/// reports <see cref="Warehouse.OvenTests"/>, four tests of a second each that run at the same time. The option
/// needs <c>EnableMicrosoftTestingExtensionsHtmlReport</c> in the project file.
/// </summary>
[TestClass]
public sealed partial class HtmlDurationRunTests(TestContext testContext)
{
    private const string Html = "oven.html";

    [TestMethod]
    public async Task Summary_duration_is_shorter_than_the_durations_of_the_tests_together()
    {
        using var run = await TestApplicationRun.StartAsync(
            testContext.CancellationToken,
            "--filter", $"ClassName={typeof(Warehouse.OvenTests).FullName}",
            "--report-html", "--report-html-filename", Html);
        var page = await File.ReadAllTextAsync(run.PathOf(Html), testContext.CancellationToken);

        var summary = Milliseconds(TotalDuration().Match(page));
        var tests = TestDuration().Matches(page).Select(Milliseconds).ToArray();

        Assert.HasCount(4, tests);
        Assert.IsGreaterThanOrEqualTo(1000, tests.Min());
        Assert.IsLessThan(tests.Sum(), summary);
    }

    private static double Milliseconds(Match match) => double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);

    [GeneratedRegex("\"totalDurationMs\":([0-9.]+)")]
    private static partial Regex TotalDuration();

    [GeneratedRegex("\"durationMs\":([0-9.]+)")]
    private static partial Regex TestDuration();
}
