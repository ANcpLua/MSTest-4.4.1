using MSTest.Showcase;

namespace MSTest.Retry.Reporting;

/// <summary>
/// <c>[Retry]</c> runs a test that failed or timed out again, here up to three more times, and stops at the
/// first attempt that does neither. <see cref="TestContext.TestRunCount"/> is the number of the attempt that
/// is running and starts at 1. Expected outcome: the three tests pass, the second after one failed attempt
/// and the third after two, and the run ends with exit code 0.
/// </summary>
[TestClass]
[Explicit]
public sealed class UnstableTests(TestContext testContext)
{
    private const int Retries = 3;

    [TestMethod]
    [Retry(Retries)]
    public void Passes_on_the_first_attempt() => Assert.AreEqual(1, testContext.TestRunCount);

    [TestMethod]
    [Retry(Retries)]
    public void Passes_on_the_second_attempt() => Assert.AreEqual(2, testContext.TestRunCount);

    [TestMethod]
    [Retry(Retries)]
    public void Passes_on_the_third_attempt() => Assert.AreEqual(3, testContext.TestRunCount);
}
