using System.Diagnostics;

namespace MSTest.Execution.Output;

/// <summary>
/// Each test writes one line and then waits for a line on the standard input, so the reader of the output decides
/// when the test finishes. Expected outcome: each test passes as soon as the standard input delivers a line or is
/// closed, and the four lines are in the output of the run. None of them is in the output when
/// <c>--config-file</c> names Output/result.testconfig.json, where <c>captureTrace</c> is <c>Result</c>, and only
/// the two lines of <see cref="Console"/> are when it names Output/none.testconfig.json, where it is <c>None</c>.
/// </summary>
[TestClass]
public sealed class ImportProgress(TestContext testContext)
{
    public const string ConsoleLine = "console: importing customers";
    public const string ErrorLine = "error: importing customers";
    public const string TraceLine = "trace: importing customers";
    public const string TestContextLine = "test context: importing customers";

    [TestMethod]
    [Interactive]
    public void Written_to_the_console()
    {
        Console.WriteLine(ConsoleLine);
        Console.ReadLine();
    }

    [TestMethod]
    [Interactive]
    public void Written_to_the_error_stream()
    {
        Console.Error.WriteLine(ErrorLine);
        Console.ReadLine();
    }

    [TestMethod]
    [Interactive]
    public void Written_to_the_trace()
    {
        Trace.WriteLine(TraceLine);
        Console.ReadLine();
    }

    [TestMethod]
    [Interactive]
    public void Written_to_the_test_context()
    {
        testContext.WriteLine(TestContextLine);
        Console.ReadLine();
    }
}
