using MSTest.Analyzers.CurrentBase;
using MSTest.Analyzers.LegacyBase;

namespace MSTest.Analyzers.InheritedMembers;

/// <summary>
/// MSTEST0082 reports a test class for each <c>[TestInitialize]</c> and <c>[TestCleanup]</c> method that it
/// inherits from a base class compiled against MSTest 3. Neither method is run: the test finds no session
/// open, and no session is closed after it.
/// </summary>
[TestClass]
#pragma warning disable MSTEST0082
public sealed class LegacySessionTests : LegacySessionChecks
#pragma warning restore MSTEST0082
{
    [TestMethod]
    public void Inherited_initialize_is_not_run() => Assert.IsFalse(SessionIsOpen);

    [ClassCleanup]
    public static void Inherited_cleanup_was_not_run() => Assert.AreEqual(0, ClosedSessions);
}

/// <summary>
/// The base class is compiled against the MSTest version of the test project, and both inherited methods are
/// run: the test finds the session open, and the session is closed after it.
/// </summary>
[TestClass]
public sealed class SessionTests : SessionChecks
{
    [TestMethod]
    public void Inherited_initialize_is_run() => Assert.IsTrue(SessionIsOpen);

    [ClassCleanup]
    public static void Inherited_cleanup_was_run() => Assert.AreEqual(1, ClosedSessions);
}
