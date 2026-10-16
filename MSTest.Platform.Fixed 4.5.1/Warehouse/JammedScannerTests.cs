using MSTest.Showcase;

namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>
/// A test that passes at once and a test that blocks its thread for ten seconds and does not watch for
/// cancellation. Expected outcome with MSTEST_EXPLICIT=1, a filter on this class, <c>--timeout 5s</c> and
/// <c>--report-trx</c>: the run is cancelled after five seconds, Waits_for_the_jammed_scanner goes on until its
/// ten seconds are over, the TRX report holds the result of Scans_the_first_pallet and the run ends with exit
/// code 7.
/// </summary>
[TestClass]
[Explicit]
public sealed class JammedScannerTests
{
    private static readonly string[] Pallet = ["apples", "pears"];

    [TestMethod]
    public void Scans_the_first_pallet() => Assert.HasCount(2, Pallet);

    [TestMethod]
    public void Waits_for_the_jammed_scanner()
    {
        Thread.Sleep(TimeSpan.FromSeconds(10));

        Assert.HasCount(2, Pallet);
    }
}
