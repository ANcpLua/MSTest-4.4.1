namespace MSTest.Isolation.ResourceLocks;

/// <summary>
///     A test that uses several resources carries one [ResourceLock] for each of them and holds them all while it
///     runs. The transfer needs both accounts, the deposit the checking account and the interest booking the
///     savings account; the deposit and the interest booking also share the teller, so they take turns. Each test
///     waits, inside its locks, until the tests that share an account with it have started, and none of them
///     meets another test on an account.
/// </summary>
[TestClass]
public sealed class MultipleLocksTests(TestContext testContext)
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(2);

    private static readonly TaskCompletionSource TransferHasStarted =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static readonly TaskCompletionSource DepositHasStarted =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static readonly TaskCompletionSource InterestHasStarted =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static int _usersOfChecking;

    private static int _usersOfSavings;

    [TestMethod]
    [ResourceLock(SharedResources.CheckingAccount)]
    [ResourceLock(SharedResources.SavingsAccount)]
    public async Task Transfer_uses_both_accounts_alone()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref _usersOfChecking));
        Assert.AreEqual(1, Interlocked.Increment(ref _usersOfSavings));
        TransferHasStarted.SetResult();

        await Task.WhenAny(
            Task.WhenAll(DepositHasStarted.Task, InterestHasStarted.Task),
            Task.Delay(Patience, testContext.CancellationToken));

        Assert.AreEqual(0, Interlocked.Decrement(ref _usersOfSavings));
        Assert.AreEqual(0, Interlocked.Decrement(ref _usersOfChecking));
    }

    [TestMethod]
    [ResourceLock(SharedResources.CheckingAccount)]
    [ResourceLock(SharedResources.Teller)]
    public async Task Deposit_uses_the_checking_account_alone()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref _usersOfChecking));
        DepositHasStarted.SetResult();

        await Task.WhenAny(TransferHasStarted.Task, Task.Delay(Patience, testContext.CancellationToken));

        Assert.AreEqual(0, Interlocked.Decrement(ref _usersOfChecking));
    }

    [TestMethod]
    [ResourceLock(SharedResources.SavingsAccount)]
    [ResourceLock(SharedResources.Teller)]
    public async Task Interest_uses_the_savings_account_alone()
    {
        Assert.AreEqual(1, Interlocked.Increment(ref _usersOfSavings));
        InterestHasStarted.SetResult();

        await Task.WhenAny(TransferHasStarted.Task, Task.Delay(Patience, testContext.CancellationToken));

        Assert.AreEqual(0, Interlocked.Decrement(ref _usersOfSavings));
    }
}