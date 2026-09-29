namespace MSTest.Isolation.ResourceLocks;

/// <summary>
/// A resource lock holds back the tests that name its resource and no others. The test without the lock
/// starts while the vault is held, and the test that holds the vault ends only after the other has run.
/// </summary>
[TestClass]
public sealed class UnrelatedTestTests(TestContext testContext)
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    private static readonly SemaphoreSlim VaultIsHeld = new(0);

    private static readonly SemaphoreSlim UnrelatedTestHasRun = new(0);

    [TestMethod]
    [ResourceLock(SharedResources.Vault)]
    public async Task Holds_the_vault_until_the_unrelated_test_has_run()
    {
        VaultIsHeld.Release();

        Assert.IsTrue(await UnrelatedTestHasRun.WaitAsync(Patience, testContext.CancellationToken));
    }

    [TestMethod]
    public async Task Runs_while_the_vault_is_held()
    {
        Assert.IsTrue(await VaultIsHeld.WaitAsync(Patience, testContext.CancellationToken));

        UnrelatedTestHasRun.Release();
    }
}
