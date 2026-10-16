using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Helpers;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.Services;
using MSTest.Platform.Artifacts;
using MSTest.Platform.Assertions;
using MSTest.Platform.Configuration;
using MSTest.Platform.Coverage;
using MSTest.Platform.Filtering;
using MSTest.Platform.Retries;
using MSTest.Platform.Services;

namespace MSTest.Platform;

/// <summary>
/// The entry point of the test application, written by hand because the extensions of this project are
/// registered in code. <c>AddSelfRegisteredExtensions</c> registers MSTest and the extensions of the packages
/// that the project references, as the generated entry point does.
/// </summary>
internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = await TestApplication.CreateBuilderAsync(args);

        builder.Configuration.AddConfigurationSource(static () => new CarrierDefaults());
        builder.AddSelfRegisteredExtensions(args);
        builder.AddTestExecutionFilterProvider(static _ => new TestListFilter(
            "ChangedTests", TestListFilter.ChangedTests, TestExecutionRequestKind.Discovery, TestExecutionRequestKind.Run));
        builder.AddTestExecutionFilterProvider(static _ => new TestListFilter(
            "NightlyTests", TestListFilter.NightlyTests, TestExecutionRequestKind.Run));
        builder.TestHost.AddTestSessionLifetimeHandler(static services => new TestHostServices(services));
        builder.TestHost.AddTestSessionLifetimeHandler(static services => new FixedCoverageCollector(services));
        builder.TestHost.AddTestHostApplicationLifetime(static services => new CoverageDigest(services));
        builder.TestHost.AddDataConsumer(static services => new AttemptReporter(services.GetOutputDevice()));
        builder.TestHost.AddDataConsumer(static services => new AssertionValueReporter(services.GetOutputDevice()));

        var outcomeLog = new CompositeExtensionFactory<OutcomeLog>(static services => new OutcomeLog(services));
        builder.TestHost.AddDataConsumer(outcomeLog);
        builder.TestHost.AddTestSessionLifetimeHandler(outcomeLog);
        ((IArtifactPostProcessingApplicationBuilder)builder).ArtifactPostProcessing.AddArtifactPostProcessor(static _ => new OutcomeLogMerger());

        using var application = await builder.BuildAsync();

        return await application.RunAsync();
    }
}
