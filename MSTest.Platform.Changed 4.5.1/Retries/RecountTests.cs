using MSTest.Showcase;

namespace MSTest.Platform.Changed.Retries;

/// <summary>
/// A test that <c>[Retry]</c> runs a second time in the same test host, because its first attempt fails.
/// <see cref="TestContext.TestRunCount"/> is the number of the attempt that is running and starts at 1.
/// This deliberately failing fixture is executed by <see cref="AttemptRetryRunTests"/>, which verifies the
/// final result and the failed attempt in real reports. It follows the same explicit-fixture convention
/// as the other retry and failure demonstrations.
/// </summary>
[TestClass]
[Explicit]
public sealed class RecountTests(TestContext testContext)
{
    [TestMethod]
    [Retry(1)]
    public void Agrees_on_the_second_count() => Assert.AreEqual(2, testContext.TestRunCount);
}
