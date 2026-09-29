#pragma warning disable MSTESTEXP

namespace MSTest.Retry.Attempts;

/// <summary>
/// A retry policy of its own: the test runs again until every result of the latest attempt has passed or the
/// retries are used up. The <see cref="RetryResult"/> is the state of the loop, because
/// <see cref="RetryResult.AllResults"/> holds the attempts added so far with the latest one last. MSTest calls
/// <see cref="RetryBaseAttribute.ExecuteAsync"/> after the first run failed or timed out and needs at least
/// one attempt in the result, so the policy takes one retry or more.
/// <see cref="RetryBaseAttribute.ExecuteAsync"/>, <see cref="RetryContext"/> and <see cref="RetryResult"/> are
/// experimental, which the file acknowledges by disabling MSTESTEXP.
/// </summary>
public sealed class RetryUntilPassedAttribute : RetryBaseAttribute
{
    private readonly int _maxRetryAttempts;

    public RetryUntilPassedAttribute(int maxRetryAttempts)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRetryAttempts, 1);

        _maxRetryAttempts = maxRetryAttempts;
    }

    protected override async Task<RetryResult> ExecuteAsync(RetryContext retryContext)
    {
        var retry = new RetryResult();

        do
        {
            retry.AddResult(await retryContext.ExecuteTaskGetter());
        }
        while (retry.AllResults.Count < _maxRetryAttempts && !Passed(retry.AllResults[^1]));

        return retry;
    }

    private static bool Passed(TestResult[] attempt) =>
        attempt.All(static result => result.Outcome == UnitTestOutcome.Passed);
}
