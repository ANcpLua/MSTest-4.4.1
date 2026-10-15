using MSTest.Analyzers.CurrentBase;
using MSTest.Analyzers.LegacyBase;

namespace MSTest.Analyzers.ClassFixtures;

/// <summary>
///     MSTEST0082 reports a test class for each <c>[ClassInitialize]</c> and <c>[ClassCleanup]</c> method with
///     <see cref="InheritanceBehavior.BeforeEachDerivedClass" /> that it inherits from a base class compiled against
///     MSTest 3. The rule reads the inheritance behavior from the attribute, so the MSTest 3 framework assembly is a
///     reference of the compilation: the project MSTest.Analyzers.ClassFixtures 4.4.1 builds this file. Neither
///     class fixture is run: the test finds no store opened, and the assembly cleanup finds no store closed.
/// </summary>
// The dual-framework compilation confuses CA1822; MSTest test methods retain their required instance shape.
#pragma warning disable CA1822
[TestClass]
#pragma warning disable MSTEST0082
public sealed class LegacyStoreTests : LegacyStoreChecks
#pragma warning restore MSTEST0082
{
    [TestMethod]
    public void Inherited_class_initialize_is_not_run()
    {
        Assert.AreEqual(0, OpenedStores);
    }
}

/// <summary>
///     The base class is compiled against the MSTest version of the test project, and both inherited class
///     fixtures are run: the store is opened before the test, and the assembly cleanup finds every opened store
///     closed. It finds no store closed for <see cref="LegacyStoreTests" />.
/// </summary>
[TestClass]
public sealed class StoreTests : StoreChecks
{
    [TestMethod]
    public void Inherited_class_initialize_is_run()
    {
        Assert.AreEqual(1, OpenedStores);
    }

    [AssemblyCleanup]
    public static void Inherited_class_cleanup_was_run_for_this_class_alone()
    {
        Assert.AreEqual(OpenedStores, ClosedStores);
        Assert.AreEqual(0, LegacyStoreChecks.ClosedStores);
    }
}
#pragma warning restore CA1822
