using System.Globalization;
using MSTest.Showcase;

namespace MSTest.Platform.Deadline;

/// <summary>
/// Three tests that run one after the other. The first one to start waits until the platform has asked for
/// the stop, which is the deadline of the run less its margin, so the stop arrives while that test runs.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: all three tests pass and the run ends
/// with exit code 0. With <c>TESTINGPLATFORM_DEADLINE</c> set to a time a few seconds ahead and
/// <c>TESTINGPLATFORM_DEADLINE_STOP_MARGIN</c> set to 0, one test passes, the others do not run and the run
/// ends with exit code 15.
/// </summary>
[TestClass]
[Explicit]
[DoNotParallelize]
public sealed class QueueTests(TestContext testContext)
{
    public const string Deadline = "TESTINGPLATFORM_DEADLINE";
    public const string StopMargin = "TESTINGPLATFORM_DEADLINE_STOP_MARGIN";

    private static readonly TimeSpan AfterTheStop = TimeSpan.FromSeconds(1);

    [TestMethod]
    public Task Serves_the_first_customer() => ServeAsync();

    [TestMethod]
    public Task Serves_the_second_customer() => ServeAsync();

    [TestMethod]
    public Task Serves_the_third_customer() => ServeAsync();

    private async Task ServeAsync()
    {
        if (!DateTimeOffset.TryParse(Environment.GetEnvironmentVariable(Deadline), CultureInfo.InvariantCulture, out var deadline))
        {
            return;
        }

        var margin = int.TryParse(Environment.GetEnvironmentVariable(StopMargin), CultureInfo.InvariantCulture, out var seconds) ? seconds : 60;
        var left = deadline - TimeSpan.FromSeconds(margin) + AfterTheStop - DateTimeOffset.UtcNow;

        if (left > TimeSpan.Zero)
        {
            await Task.Delay(left, testContext.CancellationToken);
        }
    }
}
