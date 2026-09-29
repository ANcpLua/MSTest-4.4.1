using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.Services;

/// <summary>
/// An extension that hands the services of the test host to the tests of this assembly, which run in the
/// process of the test host. <c>Program.cs</c> registers it with
/// <c>builder.TestHost.AddTestSessionLifetimeHandler</c>, whose factory receives the
/// <see cref="IServiceProvider"/> that every extension gets its services from.
/// </summary>
internal sealed class TestHostServices : ITestSessionLifetimeHandler
{
    public TestHostServices(IServiceProvider services) => Current = services;

    public static IServiceProvider Current { get; private set; } = null!;

    public string Uid => nameof(TestHostServices);

    public string Version => "1.0.0";

    public string DisplayName => "Test host services";

    public string Description => "Hands the services of the test host to the tests of the assembly.";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public Task OnTestSessionStartingAsync(ITestSessionContext testSessionContext) => Task.CompletedTask;

    public Task OnTestSessionFinishingAsync(ITestSessionContext testSessionContext) => Task.CompletedTask;
}
