namespace MSTest.Analyzers.CurrentBase;

/// <summary>
///     A base class of a library that is compiled against MSTest.TestFramework 4.5.1, the version of the test
///     project. It opens a session before each test and closes the session after the test.
/// </summary>
public abstract class SessionChecks
{
    protected static int ClosedSessions { get; private set; }

    protected bool SessionIsOpen { get; private set; }

    [TestInitialize]
    public void OpenSession()
    {
        SessionIsOpen = true;
    }

    [TestCleanup]
    public void CloseSession()
    {
        ClosedSessions++;
    }
}