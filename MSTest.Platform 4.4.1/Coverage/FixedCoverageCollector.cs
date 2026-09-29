using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.Services;
using Microsoft.Testing.Platform.TestHost;

namespace MSTest.Platform.Coverage;

/// <summary>
/// A collector that reports the coverage of a run with the messages of the platform. It stands in for a
/// coverage tool: it measures nothing and reports the number of covered lines that the variable
/// <c>SHOWCASE_COVERED_LINES</c> names, out of 200. <see cref="TestCoverageMessage"/> is one measurement, the
/// covered and the coverable count of one metric in one scope. <see cref="TestCoverageThresholdMessage"/> is
/// the result of one check against a required percentage, and <see cref="TestCoverageReportMessage"/> names
/// the report file and its format. Each message names its producer, and <c>DataTypesProduced</c> declares the
/// three types. <c>Program.cs</c> registers the collector with
/// <c>builder.TestHost.AddTestSessionLifetimeHandler</c>.
/// </summary>
internal sealed class FixedCoverageCollector(IServiceProvider services) : ITestSessionLifetimeHandler, IDataProducer
{
    public const string Variable = "SHOWCASE_COVERED_LINES";
    public const string Report = "coverage.lcov";
    public const string Module = "Shop.dll";
    public const long Lines = 200;
    public const double RequiredPercentage = 80;

    private static readonly string? CoveredLines = Environment.GetEnvironmentVariable(Variable);

    public string Uid => nameof(FixedCoverageCollector);

    public string Version => "1.0.0";

    public string DisplayName => "Fixed coverage collector";

    public string Description => "Reports a fixed number of covered lines.";

    public Type[] DataTypesProduced =>
        [typeof(TestCoverageMessage), typeof(TestCoverageThresholdMessage), typeof(TestCoverageReportMessage)];

    public Task<bool> IsEnabledAsync() => Task.FromResult(CoveredLines is not null);

    public Task OnTestSessionStartingAsync(ITestSessionContext testSessionContext) => Task.CompletedTask;

    public async Task OnTestSessionFinishingAsync(ITestSessionContext testSessionContext)
    {
        var session = testSessionContext.SessionUid;
        var covered = long.Parse(CoveredLines!);
        var report = Path.Combine(services.GetConfiguration().GetTestResultDirectory(), Report);
        await File.WriteAllLinesAsync(report, ["SF:Shop/Basket.cs", $"LF:{Lines}", $"LH:{covered}", "end_of_record"], testSessionContext.CancellationToken);

        await PublishAsync(new TestCoverageMessage(session, CoverageScope.Overall, CoverageMetric.Line, covered, Lines, Uid));
        await PublishAsync(new TestCoverageMessage(session, CoverageScope.Overall, CoverageMetric.Branch, coveredCount: 30, coverableCount: 40, Uid));
        await PublishAsync(new TestCoverageMessage(session, new CoverageScope(CoverageScopeLevel.Module, Module), CoverageMetric.Line, covered, Lines, Uid));
        await PublishAsync(new TestCoverageMessage(session, new CoverageScope(CoverageScopeLevel.Type, "Shop.Basket", containerHint: Module), CoverageMetric.Line, covered, Lines, Uid));
        await PublishAsync(new TestCoverageThresholdMessage(
            session,
            CoverageScope.Overall,
            CoverageMetric.Line,
            CoverageAggregation.None,
            actualPercentage: 100d * covered / Lines,
            RequiredPercentage,
            hasCoverableData: true,
            Uid));
        await PublishAsync(new TestCoverageReportMessage(session, report, CoverageReportFormat.Lcov, Uid));
    }

    private Task PublishAsync(DataWithSessionUid message) => services.GetMessageBus().PublishAsync(this, message);
}
