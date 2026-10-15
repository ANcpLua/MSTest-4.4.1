namespace MSTest.Analyzers.LegacyBase;

/// <summary>
///     A base class of a library that is compiled against MSTest.TestFramework 3.11.1. It opens a store before the
///     tests of each derived class and closes the store after them.
/// </summary>
public abstract class LegacyStoreChecks
{
    protected static int OpenedStores { get; private set; }

    // Observed from the current-framework test assembly, outside this inheritance hierarchy.
    // ReSharper disable once MemberCanBeProtected.Global
    public static int ClosedStores { get; private set; }

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void OpenStore(TestContext context)
    {
        OpenedStores++;
    }

    [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void CloseStore()
    {
        ClosedStores++;
    }
}