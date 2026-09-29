namespace MSTest.Analyzers.LegacyBase;

/// <summary>
/// A base class of a library that is compiled against MSTest.TestFramework 3.11.1. It opens a session before
/// each test and closes the session after the test.
/// </summary>
public abstract class LegacySessionChecks
{
    public static int ClosedSessions { get; private set; }

    public bool SessionIsOpen { get; private set; }

    [TestInitialize]
    public void OpenSession() => SessionIsOpen = true;

    [TestCleanup]
    public void CloseSession() => ClosedSessions++;
}
