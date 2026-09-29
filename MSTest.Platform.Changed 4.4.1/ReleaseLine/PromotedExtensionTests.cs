using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Testing.Extensions;
using Microsoft.Testing.Platform.Builder;

namespace MSTest.Platform.Changed.ReleaseLine;

/// <summary>
/// The OpenTelemetry, the PackagedApp and the GitHub Actions report extension belong to the release line of
/// the platform: their assemblies have the version of Microsoft.Testing.Platform, and the methods that
/// register them are not experimental, so a call needs no suppression of <c>TPEXP</c>. The packages come
/// with <c>EnableMicrosoftTestingExtensionsOpenTelemetry</c>,
/// <c>EnableMicrosoftTestingExtensionsPackagedApp</c> and
/// <c>EnableMicrosoftTestingExtensionsGitHubActionsReport</c> in the project file.
/// </summary>
[TestClass]
public sealed class PromotedExtensionTests
{
    private static readonly string Platform = ReleaseOf(typeof(TestApplication));

    [TestMethod]
    [DataRow(typeof(OpenTelemetryProviderExtensions))]
    [DataRow(typeof(PackagedAppExtensions))]
    [DataRow(typeof(GitHubActionsExtensions))]
    public void Extension_has_the_version_of_the_platform(Type registration)
    {
        Assert.AreEqual("2.4.1", Platform);
        Assert.AreEqual(Platform, ReleaseOf(registration));
    }

    [TestMethod]
    [DataRow(typeof(OpenTelemetryProviderExtensions), nameof(OpenTelemetryProviderExtensions.AddOpenTelemetryProvider))]
    [DataRow(typeof(OpenTelemetryProviderExtensions), nameof(OpenTelemetryProviderExtensions.AddOpenTelemetryProviderFromEnvironment))]
    [DataRow(typeof(PackagedAppExtensions), nameof(PackagedAppExtensions.AddPackagedAppDeployment))]
    [DataRow(typeof(GitHubActionsExtensions), nameof(GitHubActionsExtensions.AddGitHubActionsProvider))]
    public void Registration_is_not_experimental(Type registration, string method)
    {
        Assert.IsNull(registration.GetCustomAttribute<ExperimentalAttribute>());
        Assert.IsNull(registration.GetMethod(method)!.GetCustomAttribute<ExperimentalAttribute>());
    }

    private static string ReleaseOf(Type type)
    {
        var version = type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        return version[..version.IndexOf('+')];
    }
}
