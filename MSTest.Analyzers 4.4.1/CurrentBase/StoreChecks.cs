namespace MSTest.Analyzers.CurrentBase;

/// <summary>
/// A base class of a library that is compiled against MSTest.TestFramework 4.5.1, the version of the test
/// project. It opens a store before the tests of each derived class and closes the store after them.
/// </summary>
public abstract class StoreChecks
{
    public static int OpenedStores { get; private set; }

    public static int ClosedStores { get; private set; }

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void OpenStore(TestContext context) => OpenedStores++;

    [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void CloseStore() => ClosedStores++;
}
