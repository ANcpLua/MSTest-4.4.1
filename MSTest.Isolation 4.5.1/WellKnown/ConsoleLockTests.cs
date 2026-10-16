namespace MSTest.Isolation.WellKnown;

/// <summary>
///     WellKnownResources.Console is the key for the console of the process. Each row redirects Console.Out and
///     finds only its own line in the writer because the other row waits for the lock.
/// </summary>
[TestClass]
public sealed class ConsoleLockTests(TestContext testContext)
{
    private static readonly TimeSpan Hold = TimeSpan.FromMilliseconds(200);

    [TestMethod]
    [ResourceLock(WellKnownResources.Console)]
    [DataRow("first line")]
    [DataRow("second line")]
    public async Task Captures_the_line_it_wrote(string line)
    {
        var previous = Console.Out;
        await using StringWriter captured = new();
        Console.SetOut(captured);

        try
        {
            await Task.Delay(Hold, testContext.CancellationToken);

            Console.Write(line);

            Assert.AreEqual(line, captured.ToString());
        }
        finally
        {
            Console.SetOut(previous);
        }
    }
}
