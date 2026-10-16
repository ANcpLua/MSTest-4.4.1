using MSTest.Showcase;

using System.Collections.Concurrent;

namespace MSTest.Ordering.Dependencies;

/// <summary>
/// [DependsOn(typeof(...))] waits for every test of another class, and
/// [DependsOn(typeof(...), nameof(...))] for one test of another class. ImportCustomers starts after
/// ImportOrders and takes longer, so a test that waits only for ImportOrders finds ImportCustomers unfinished.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class ReportTests
{
    [TestMethod]
    [DependsOn(typeof(ImportTests))]
    public void Runs_after_every_test_of_the_class() =>
        Assert.AreSequenceEqual(
            [nameof(ImportTests.ImportCustomers), nameof(ImportTests.ImportOrders)],
            ImportTests.Finished,
            SequenceOrder.InAnyOrder);

    [TestMethod]
    [DependsOn(typeof(ImportTests), nameof(ImportTests.ImportCustomers))]
    public void Runs_after_the_named_test_of_the_class() =>
        Assert.Contains(nameof(ImportTests.ImportCustomers), ImportTests.Finished);
}

/// <summary>The tests that <see cref="ReportTests"/> names by their class.</summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class ImportTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);

    internal static ConcurrentQueue<string> Finished { get; } = new();

    [TestMethod]
    [DependsOn(nameof(ImportOrders))]
    public async Task ImportCustomers()
    {
        await Task.Delay(3 * Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(ImportCustomers));
    }

    [TestMethod]
    public async Task ImportOrders()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(ImportOrders));
    }
}
