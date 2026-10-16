using MSTest.Showcase;

namespace MSTest.Retry.DataDriven;

/// <summary>
/// With <see cref="TestDataSourceUnfoldingStrategy.Fold"/> the rows of a data-driven test run as one test, so
/// every attempt has one result per row. <c>[Retry]</c> runs all rows again while a row of the attempt failed
/// or timed out, and an inconclusive row ends the retries only when no other row of its attempt did. Without
/// folding every row is a test with attempts of its own, where an inconclusive result ends the retries.
/// Expected outcome: the three unstable rows pass on their second attempt, the four other results are
/// inconclusive, and the run ends with exit code 0.
/// </summary>
[TestClass]
[Explicit]
public sealed class InconclusiveRowTests(TestContext testContext)
{
    private const int Retries = 3;
    private const int Limit = 1000;
    private const string Inconclusive = "inconclusive";
    private const string Unstable = "unstable";
    private const string NotDeployed = "The region is not deployed.";

    [TestMethod(UnfoldingStrategy = TestDataSourceUnfoldingStrategy.Fold)]
    [Retry(Retries)]
    [DataRow(Inconclusive)]
    [DataRow(Unstable)]
    public void Rows_run_again_while_one_of_them_failed(string row)
    {
        if (row == Inconclusive)
        {
            Assert.Inconclusive(NotDeployed);
        }

        Assert.AreEqual(2, testContext.TestRunCount);
    }

    [TestMethod(UnfoldingStrategy = TestDataSourceUnfoldingStrategy.Fold)]
    [Retry(Retries)]
    [Timeout(Limit, CooperativeCancellation = true)]
    [DataRow(Inconclusive)]
    [DataRow(Unstable)]
    public async Task Rows_run_again_while_one_of_them_timed_out(string row)
    {
        if (row == Inconclusive)
        {
            Assert.Inconclusive(NotDeployed);
        }

        if (testContext.TestRunCount == 1)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, testContext.CancellationToken);
        }

        Assert.AreEqual(2, testContext.TestRunCount);
    }

    [TestMethod]
    [Retry(Retries)]
    [DataRow(Inconclusive)]
    [DataRow(Unstable)]
    public void Unfolded_rows_are_retried_on_their_own(string row)
    {
        if (row == Inconclusive)
        {
            Assert.AreEqual(1, testContext.TestRunCount);
            Assert.Inconclusive(NotDeployed);
        }

        Assert.AreEqual(2, testContext.TestRunCount);
    }

    [TestMethod]
    [Retry(Retries)]
    public void An_inconclusive_result_on_its_own_ends_the_retries()
    {
        Assert.AreEqual(2, testContext.TestRunCount);
        Assert.Inconclusive(NotDeployed);
    }
}
