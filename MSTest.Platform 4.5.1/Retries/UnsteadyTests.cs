using MSTest.Showcase;

namespace MSTest.Platform.Retries;

/// <summary>
/// Two scans that <c>[Retry]</c> repeats within the run: one passes at its third attempt, the other passes at
/// once. Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: both tests pass, the first
/// after two failed attempts, and the run ends with exit code 0.
/// </summary>
[TestClass]
[Explicit]
public sealed class UnsteadyTests(TestContext testContext)
{
    [TestMethod]
    [Retry(2)]
    public void Scans_a_label_at_the_third_attempt() => Assert.AreEqual(3, testContext.TestRunCount);

    [TestMethod]
    [Retry(2)]
    public void Scans_a_label_at_once() => Assert.AreEqual(1, testContext.TestRunCount);
}
