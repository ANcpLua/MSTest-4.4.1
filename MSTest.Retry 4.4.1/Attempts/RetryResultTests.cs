#pragma warning disable MSTESTEXP

namespace MSTest.Retry.Attempts;

/// <summary>
/// <see cref="RetryResult.AllResults"/> holds one element per attempt that was added with
/// <see cref="RetryResult.AddResult"/>, in the order of the calls. <see cref="RetryResult"/> is experimental,
/// which the file acknowledges by disabling MSTESTEXP.
/// </summary>
[TestClass]
public sealed class RetryResultTests
{
    [TestMethod]
    public void AllResults_lists_every_attempt_in_the_order_it_was_added()
    {
        TestResult[] second = [new() { Outcome = UnitTestOutcome.Timeout }];
        TestResult[] third = [new() { Outcome = UnitTestOutcome.Passed }];
        var retry = new RetryResult();

        retry.AddResult(second);
        retry.AddResult(third);

        Assert.HasCount(2, retry.AllResults);
        Assert.AreSame(second, retry.AllResults[0]);
        Assert.AreSame(third, retry.AllResults[1]);
    }
}
