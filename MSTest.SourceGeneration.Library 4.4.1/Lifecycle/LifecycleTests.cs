using System.Collections.Concurrent;

namespace MSTest.SourceGeneration.Library.Lifecycle;

/// <summary>
/// The base class of a test class of another assembly, with the members that MSTest reaches through the
/// derived class: the <see cref="TestContext"/> property, a test initializer, a class initializer that runs
/// for every derived class and a protected data source.
/// </summary>
public abstract class LifecycleTests
{
    protected static ConcurrentBag<string> InitializedClasses { get; } = [];

    protected static IEnumerable<object[]> Squares => [[2, 4], [3, 9]];

    public TestContext TestContext { get; set; } = null!;

    protected string? InitializedTest { get; private set; }

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void RecordTheClass(TestContext testContext) => InitializedClasses.Add(testContext.FullyQualifiedTestClassName!);

    [TestInitialize]
    public void RecordTheTest() => InitializedTest = TestContext.TestName;
}
