using System.Collections.Concurrent;

namespace MSTest.Ordering.Dependencies;

/// <summary>
/// [DependsOn] on a class gives the dependency to every test of the class. Naming a test of the class
/// itself makes every other test of the class wait for it.
/// </summary>
[TestClass]
[DependsOn(nameof(SignIn))]
public sealed class AccountTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);

    internal static ConcurrentQueue<string> Finished { get; } = new();

    [TestMethod]
    public async Task ChangeEmail()
    {
        Assert.Contains(nameof(SignIn), Finished);

        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(ChangeEmail));
    }

    [TestMethod]
    [DependsOn(nameof(ChangeEmail))]
    public async Task ChangePassword()
    {
        Assert.Contains(nameof(SignIn), Finished);

        await Task.Delay(3 * Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(ChangePassword));
    }

    [TestMethod]
    public async Task SignIn()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(SignIn));
    }
}

/// <summary>
/// [DependsOn(typeof(...))] on a class makes every test of the class wait for every test of the other
/// class. ChangePassword starts after ChangeEmail and takes longer, so a test that waits only for
/// ChangeEmail finds ChangePassword unfinished.
/// </summary>
[TestClass]
[DependsOn(typeof(AccountTests))]
public sealed class ClosedAccountTests
{
    private static readonly string[] AccountTestNames =
    [
        nameof(AccountTests.SignIn), nameof(AccountTests.ChangeEmail), nameof(AccountTests.ChangePassword)
    ];

    [TestMethod]
    public void CloseAccount() =>
        Assert.AreSequenceEqual(AccountTestNames, AccountTests.Finished, SequenceOrder.InAnyOrder);

    [TestMethod]
    public void DeleteData() =>
        Assert.AreSequenceEqual(AccountTestNames, AccountTests.Finished, SequenceOrder.InAnyOrder);
}
