using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.TestHostControllers;

namespace MSTest.Platform.Fixed.Controller;

/// <summary>
/// An extension of the controller, the process that starts the test host, that gives the test host the startup
/// hooks named in <c>MSTEST_TEST_HOST_STARTUP_HOOKS</c> as <c>DOTNET_STARTUP_HOOKS</c>. An
/// <c>ITestHostEnvironmentVariableProvider</c> sets variables for the test host alone, so the controller starts
/// without them. The extension is enabled only when the variable is set.
/// </summary>
internal sealed class TestHostStartupHooks : ITestHostEnvironmentVariableProvider
{
    public const string Variable = "MSTEST_TEST_HOST_STARTUP_HOOKS";

    public string Uid => nameof(TestHostStartupHooks);

    public string Version => "1.0.0";

    public string DisplayName => "Startup hooks of the test host";

    public string Description => $"Sets DOTNET_STARTUP_HOOKS of the test host to {Variable}.";

    public Task<bool> IsEnabledAsync() => Task.FromResult(Environment.GetEnvironmentVariable(Variable) is not null);

    public Task UpdateAsync(IEnvironmentVariables environmentVariables)
    {
        environmentVariables.SetVariable(
            new EnvironmentVariable("DOTNET_STARTUP_HOOKS", Environment.GetEnvironmentVariable(Variable), isSecret: false, isLocked: true));

        return Task.CompletedTask;
    }

    public Task<ValidationResult> ValidateTestHostEnvironmentVariablesAsync(IReadOnlyEnvironmentVariables environmentVariables) =>
        Task.FromResult(ValidationResult.Valid());
}
