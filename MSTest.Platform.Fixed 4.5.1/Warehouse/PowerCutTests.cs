using System.Diagnostics;
using MSTest.Showcase;

namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>
/// A test that passes and a test that ends the test host three seconds later, as a crash does, before the run
/// can write its reports. Expected outcome with MSTEST_EXPLICIT=1, a
/// filter on this class and <c>--report-junit</c>: Counts_the_stock passes, the test host ends during
/// Loses_the_power, the report holds the result of Counts_the_stock and is marked incomplete, and the run ends
/// with exit code 7.
/// </summary>
[TestClass]
[Explicit]
public sealed class PowerCutTests(TestContext testContext)
{
    private static readonly string[] Stock = ["apples", "pears"];

    [TestMethod]
    public void Counts_the_stock() => Assert.HasCount(2, Stock);

    [TestMethod]
    public async Task Loses_the_power()
    {
        await Task.Delay(TimeSpan.FromSeconds(3), testContext.CancellationToken);

        Process.GetCurrentProcess().Kill();
    }
}
