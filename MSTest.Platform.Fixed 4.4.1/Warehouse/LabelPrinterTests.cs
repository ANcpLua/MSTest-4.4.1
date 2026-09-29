using MSTest.Showcase;

namespace MSTest.Platform.Fixed.Warehouse;

/// <summary>
/// A test that <c>[Retry]</c> runs again in the same test host. Each attempt prints a label into a file and
/// attaches it with <c>TestContext.AddResultFile</c>, and the first attempt fails because the printer jams.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: the first attempt fails, the second
/// passes and the run ends with exit code 0.
/// </summary>
[TestClass]
[Explicit]
public sealed class LabelPrinterTests(TestContext testContext)
{
    private static int _attempts;

    [TestMethod]
    [Retry(1)]
    public void Prints_the_label_at_the_second_attempt()
    {
        var attempt = Interlocked.Increment(ref _attempts);
        var label = Path.Combine(Directory.CreateTempSubdirectory().FullName, $"label-{attempt}.txt");
        File.WriteAllText(label, "Pallet 7");

        testContext.AddResultFile(label);

        Assert.AreEqual(2, attempt, "The printer jams at the first attempt.");
    }
}
