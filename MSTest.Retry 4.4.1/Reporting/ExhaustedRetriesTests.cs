using MSTest.Showcase;

namespace MSTest.Retry.Reporting;

/// <summary>
/// A test that fails in its first run and in both retries. Expected outcome: the test fails after three
/// attempts and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class ExhaustedRetriesTests(TestContext testContext)
{
    private const int Retries = 2;

    [TestMethod]
    [Retry(Retries)]
    public void Fails_on_every_attempt() => Assert.Fail($"Attempt {testContext.TestRunCount} failed.");
}
