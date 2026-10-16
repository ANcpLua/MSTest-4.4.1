using MSTest.Showcase;

namespace MSTest.Platform.Deadline;

/// <summary>
/// A test that is stuck for twenty seconds and does not react to a request to stop. Expected outcome with
/// MSTEST_EXPLICIT=1 and a filter on this class: the test passes after twenty seconds and the run ends with
/// exit code 0. With <c>--hangdump</c> and a deadline that falls into the twenty seconds, the hang dump
/// extension ends the test host before the deadline and the run ends with exit code 7.
/// </summary>
[TestClass]
[Explicit]
public sealed class StuckTests
{
    private static readonly TimeSpan Stuck = TimeSpan.FromSeconds(20);

    [TestMethod]
    public async Task Waits_for_a_lock_that_nobody_releases() => await Task.Delay(Stuck, CancellationToken.None);
}
