using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;

namespace MSTest.Platform.Coverage;

/// <summary>
/// An extension that reads the coverage of the run after the run and prints it in one line.
/// <see cref="ITestCoverageResult"/> is the coverage that the platform collected from the messages:
/// <c>Scopes</c> has one summary for each scope with the measurements of that scope, <c>Thresholds</c> the
/// checks, <c>Reports</c> the report files and <c>HasThresholdFailure</c> tells whether a check failed.
/// <see cref="ITestCoverageCapabilities"/> names the producers that declare a coverage message. Both come
/// from the services of the platform. <c>Program.cs</c> registers the extension with
/// <c>builder.TestHost.AddTestHostApplicationLifetime</c>.
/// </summary>
internal sealed class CoverageDigest(IServiceProvider services) : ITestHostApplicationLifetime, IOutputDeviceDataProducer
{
    public string Uid => nameof(CoverageDigest);

    public string Version => "1.0.0";

    public string DisplayName => "Coverage digest";

    public string Description => "Prints the coverage of the run in one line.";

    public Task<bool> IsEnabledAsync() =>
        Task.FromResult(Environment.GetEnvironmentVariable(FixedCoverageCollector.Variable) is not null);

    public Task BeforeRunAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task AfterRunAsync(int exitCode, CancellationToken cancellationToken)
    {
        var coverage = services.GetRequiredService<ITestCoverageResult>();
        var producers = services.GetRequiredService<ITestCoverageCapabilities>().EnabledProducerUids;
        var lines = coverage.Scopes.Single(static summary => summary.Scope == CoverageScope.Overall)[CoverageMetric.Line]!;
        var report = coverage.Reports.Single();

        await services.GetOutputDevice().DisplayAsync(
            this,
            new TextOutputDeviceData(
                $"Coverage by {string.Join(", ", producers)}: {lines.CoveredCount} of {lines.CoverableCount} lines, " +
                $"{coverage.Scopes.Count} scopes, {(coverage.HasThresholdFailure ? "below" : "within")} the threshold, " +
                $"{report.Format} report {Path.GetFileName(report.Path)}"),
            cancellationToken);
    }
}
