namespace MSTest.Isolation.WellKnown;

/// <summary>
/// WellKnownResources.EnvironmentVariables is the key for the environment variables of the process. Both rows
/// set the same variable, and each reads its own value back because the other row waits for the lock.
/// </summary>
[TestClass]
public sealed class EnvironmentVariablesLockTests(TestContext testContext)
{
    private const string Region = "MSTEST_ISOLATION_REGION";

    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    [TestMethod]
    [ResourceLock(WellKnownResources.EnvironmentVariables)]
    [DataRow("north")]
    [DataRow("south")]
    public async Task Reads_back_the_region_it_set(string region)
    {
        var previous = Environment.GetEnvironmentVariable(Region);
        try
        {
            Environment.SetEnvironmentVariable(Region, region);
            await Task.Delay(Hold, testContext.CancellationToken);
            Assert.AreEqual(region, Environment.GetEnvironmentVariable(Region));
        }
        finally
        {
            Environment.SetEnvironmentVariable(Region, previous);
        }
    }
}
