using MSTest.Showcase;

using System.Collections.Concurrent;

namespace MSTest.Ordering.Configuration;

/// <summary>
/// "Namespace.Class.*" stands for every test of a class. The node in testconfig.json uses it for the tests
/// that wait and for the tests they wait for, so each test of this class runs after both deployments.
/// A second node lets DeployWeb start after DeployApi and take longer, so a test that waits only for
/// DeployApi finds DeployWeb unfinished.
/// </summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class SmokeTests
{
    private static readonly string[] Deployments =
    [
        nameof(DeploymentTests.DeployApi), nameof(DeploymentTests.DeployWeb)
    ];

    [TestMethod]
    public void HomePage() =>
        Assert.AreSequenceEqual(Deployments, DeploymentTests.Finished, SequenceOrder.InAnyOrder);

    [TestMethod]
    public void SignInPage() =>
        Assert.AreSequenceEqual(Deployments, DeploymentTests.Finished, SequenceOrder.InAnyOrder);
}

/// <summary>The tests that testconfig.json names as "MSTest.Ordering.Configuration.DeploymentTests.*".</summary>
[TestClass]
[Explicit]
[TestCategory("OrderingScenario")]
public sealed class DeploymentTests(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);

    internal static ConcurrentQueue<string> Finished { get; } = new();

    [TestMethod]
    public async Task DeployApi()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(DeployApi));
    }

    [TestMethod]
    public async Task DeployWeb()
    {
        await Task.Delay(3 * Work, testContext.CancellationToken);
        Finished.Enqueue(nameof(DeployWeb));
    }
}
