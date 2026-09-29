using System.Collections.Concurrent;

namespace MSTest.Ordering.Dependencies;

/// <summary>
/// A dependency between two tests of a base class applies in every derived test class: each of them runs
/// its own Save before its own Load.
/// </summary>
public abstract class RepositoryContract(TestContext testContext)
{
    private static readonly TimeSpan Work = TimeSpan.FromMilliseconds(100);
    private static readonly ConcurrentBag<Type> Saved = [];

    [TestMethod]
    [DependsOn(nameof(Save))]
    public void Load() => Assert.Contains(GetType(), Saved);

    [TestMethod]
    public async Task Save()
    {
        await Task.Delay(Work, testContext.CancellationToken);
        Saved.Add(GetType());
    }
}

/// <summary>Runs the two tests of <see cref="RepositoryContract"/> in the order their dependency declares.</summary>
[TestClass]
public sealed class SqlRepositoryTests(TestContext testContext) : RepositoryContract(testContext);

/// <summary>Runs the two tests of <see cref="RepositoryContract"/> in the order their dependency declares.</summary>
[TestClass]
public sealed class FileRepositoryTests(TestContext testContext) : RepositoryContract(testContext);
