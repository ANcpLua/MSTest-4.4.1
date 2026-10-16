using MSTest.Showcase;

using System.Collections.Concurrent;

namespace MSTest.Ordering.Dependencies;

/// <summary>Naming a data-driven test makes the dependent wait for every row of it.</summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class DataRowTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static readonly ConcurrentQueue<string> Imported = new();

    [TestMethod]
    [DependsOn(nameof(Import))]
    public void Runs_after_every_row() =>
        Assert.AreSequenceEqual(["customers", "orders", "invoices"], Imported, SequenceOrder.InAnyOrder);

    [TestMethod]
    [DataRow("customers")]
    [DataRow("orders")]
    [DataRow("invoices")]
    public async Task Import(string table)
    {
        await Task.Delay(Work, testContext.CancellationToken);
        Imported.Enqueue(table);
    }
}
