using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Requests;

namespace MSTest.Platform.Filtering;

/// <summary>
/// A filter provider that limits a request to the tests that an environment variable lists, separated by
/// commas. <see cref="ITestExecutionFilterProvider"/> returns the constraint of the provider for one request
/// or <see langword="null"/> when it has none. <see cref="TestExecutionFilterContext"/> tells whether the
/// request discovers or runs tests and whether it comes from the command line or from a client of the
/// server mode, whose requests take no constraint of a provider. The platform combines the filter of the
/// request and the constraints of all providers, so a test runs when every one of them accepts it. MSTest
/// takes a UID that is no GUID as the fully qualified name of a test. <c>Program.cs</c> registers two such
/// providers with <c>builder.AddTestExecutionFilterProvider</c>.
/// </summary>
internal sealed class TestListFilter(string uid, string variable, params TestExecutionRequestKind[] kinds) : ITestExecutionFilterProvider
{
    public const string ChangedTests = "SHOWCASE_CHANGED_TESTS";
    public const string NightlyTests = "SHOWCASE_NIGHTLY_TESTS";

    private readonly string? _tests = Environment.GetEnvironmentVariable(variable);

    public string Uid => uid;

    public string Version => "1.0.0";

    public string DisplayName => $"Test list of {variable}";

    public string Description => $"Limits a request to the tests that {variable} lists.";

    public Task<bool> IsEnabledAsync() => Task.FromResult(_tests is not null);

    public Task<ITestExecutionFilter?> GetFilterAsync(TestExecutionFilterContext context, CancellationToken cancellationToken) =>
        Task.FromResult<ITestExecutionFilter?>(
            context.Origin == TestExecutionRequestOrigin.Console && kinds.Contains(context.RequestKind)
                ? new TestNodeUidListFilter([.. _tests!.Split(',').Select(static test => new TestNodeUid(test))])
                : null);
}
