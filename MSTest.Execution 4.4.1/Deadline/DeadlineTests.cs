using System.Globalization;
using MSTest.Showcase;

namespace MSTest.Execution.Deadline;

/// <summary>
/// <c>TESTINGPLATFORM_DEADLINE</c> is the instant at which the job is cancelled, and
/// <c>TESTINGPLATFORM_DEADLINE_STOP_MARGIN</c> the number of seconds before that instant from which on no further
/// test is started. Each test here runs until two seconds before the deadline and fails when it starts later than
/// that. Expected outcome for a deadline 30 seconds ahead and a margin of 15 seconds: the run prints "Deadline
/// approaching: gracefully stopping the test run so reports can be finalized before the CI hard-cancel.", the test
/// that was running passes, the other one is not started, the summary counts "total: 1" and "succeeded: 1", the
/// report of <c>--report-trx</c> is written, and the exit code is 15.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class DeadlineTests(TestContext testContext)
{
    private static readonly TimeSpan BeforeTheDeadline = TimeSpan.FromSeconds(2);

    [TestMethod]
    [Explicit]
    public Task Imports_the_customers() => RunUntilShortlyBeforeTheDeadlineAsync();

    [TestMethod]
    [Explicit]
    public Task Imports_the_orders() => RunUntilShortlyBeforeTheDeadlineAsync();

    private async Task RunUntilShortlyBeforeTheDeadlineAsync()
    {
        string? deadline = Environment.GetEnvironmentVariable("TESTINGPLATFORM_DEADLINE");
        Assert.IsNotNull(deadline);

        DateTimeOffset end = DateTimeOffset.Parse(deadline, CultureInfo.InvariantCulture) - BeforeTheDeadline;
        TimeSpan remaining = end - DateTimeOffset.UtcNow;
        Assert.IsGreaterThan(TimeSpan.Zero, remaining);

        await Task.Delay(remaining, testContext.CancellationToken);
    }
}
