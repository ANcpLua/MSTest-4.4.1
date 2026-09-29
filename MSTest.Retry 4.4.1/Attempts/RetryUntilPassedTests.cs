using MSTest.Showcase;

namespace MSTest.Retry.Attempts;

/// <summary>
/// A test under <see cref="RetryUntilPassedAttribute"/> that fails, then is inconclusive and then passes. The
/// policy runs the third attempt, where <c>[Retry]</c> ends at the inconclusive one. Expected outcome: the
/// test passes on its third attempt and the run ends with exit code 0.
/// </summary>
[TestClass]
[Explicit]
public sealed class RetryUntilPassedTests(TestContext testContext)
{
    private const int Retries = 3;

    [TestMethod]
    [RetryUntilPassed(Retries)]
    public void Passes_after_a_failed_and_an_inconclusive_attempt()
    {
        switch (testContext.TestRunCount)
        {
            case 1:
                Assert.Fail("The service refused the connection.");
                break;
            case 2:
                Assert.Inconclusive("The service is still warming up.");
                break;
        }

        Assert.AreEqual(3, testContext.TestRunCount);
    }
}
