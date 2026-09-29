using System.Diagnostics;
using MSTest.Showcase;

namespace MSTest.Platform.Changed.TrxRecovery;

/// <summary>
/// A test that passes and a test that ends the test host a second later, as a crash does, before the run
/// can write its report. Expected outcome with MSTEST_EXPLICIT=1, a filter on this class and
/// <c>--report-trx</c>: Counts_the_stock passes, the test host ends during Loses_the_power, the controller
/// writes the TRX report with the result of Counts_the_stock and the run ends with exit code 7.
/// </summary>
[TestClass]
[Explicit]
public sealed class PowerCutTests(TestContext testContext)
{
    private static readonly string[] Stock = ["apple", "pear"];

    [TestMethod]
    public void Counts_the_stock() => Assert.HasCount(2, Stock);

    [TestMethod]
    public async Task Loses_the_power()
    {
        await Task.Delay(TimeSpan.FromSeconds(1), testContext.CancellationToken);

        Process.GetCurrentProcess().Kill();
    }
}
