namespace MSTest.Isolation.ClassLevel.Analyzers;

/// <summary>
///     MSTEST0074 reports a test that sets an environment variable or redirects the console without declaring
///     the lock for it. Its code fix adds [ResourceLock] with WellKnownResources.EnvironmentVariables or
///     WellKnownResources.Console to the test.
/// </summary>
[TestClass]
public sealed class ProcessGlobalStateRuleTests
{
    private const string Mode = "MSTEST_ISOLATION_MODE";

#pragma warning disable MSTEST0074
    [TestMethod]
    public void Environment_variable_set_without_a_lock_is_reported()
    {
        Environment.SetEnvironmentVariable(Mode, "reported");

        Assert.AreEqual("reported", Environment.GetEnvironmentVariable(Mode));
    }
#pragma warning restore MSTEST0074

    [TestMethod]
    [ResourceLock(WellKnownResources.EnvironmentVariables)]
    public void Environment_variable_set_under_the_lock_is_accepted()
    {
        Environment.SetEnvironmentVariable(Mode, "accepted");

        Assert.AreEqual("accepted", Environment.GetEnvironmentVariable(Mode));
    }

#pragma warning disable MSTEST0074
    [TestMethod]
    public void Console_redirected_without_a_lock_is_reported()
    {
        var previous = Console.Out;
        using StringWriter captured = new();
        Console.SetOut(captured);

        try
        {
            Console.Write("reported");

            Assert.AreEqual("reported", captured.ToString());
        }
        finally
        {
            Console.SetOut(previous);
        }
    }
#pragma warning restore MSTEST0074

    [TestMethod]
    [ResourceLock(WellKnownResources.Console)]
    public void Console_redirected_under_the_lock_is_accepted()
    {
        var previous = Console.Out;
        using StringWriter captured = new();
        Console.SetOut(captured);

        try
        {
            Console.Write("accepted");

            Assert.AreEqual("accepted", captured.ToString());
        }
        finally
        {
            Console.SetOut(previous);
        }
    }
}

/// <summary>
///     MSTEST0074 reports TestInitialize as well. There its code fix adds the lock to the class, where it covers
///     every test of the class together with TestInitialize and TestCleanup.
/// </summary>
[TestClass]
public sealed class ReportedFixtureTests
{
    private const string Stage = "MSTEST_ISOLATION_REPORTED_STAGE";

    [TestInitialize]
    public void SetTheStage()
    {
#pragma warning disable MSTEST0074
        Environment.SetEnvironmentVariable(Stage, "reported");
#pragma warning restore MSTEST0074
    }

    [TestMethod]
    public void Environment_variable_set_in_the_fixture_is_reported()
    {
        Assert.AreEqual("reported", Environment.GetEnvironmentVariable(Stage));
    }
}

/// <summary>
///     The class after the code fix of MSTEST0074: the lock stands on the class, and TestInitialize sets the
///     environment variable under it.
/// </summary>
[TestClass]
[ResourceLock(WellKnownResources.EnvironmentVariables)]
public sealed class AcceptedFixtureTests
{
    private const string Stage = "MSTEST_ISOLATION_ACCEPTED_STAGE";

    [TestInitialize]
    public void SetTheStage()
    {
        Environment.SetEnvironmentVariable(Stage, "accepted");
    }

    [TestMethod]
    public void Environment_variable_set_in_the_fixture_is_accepted()
    {
        Assert.AreEqual("accepted", Environment.GetEnvironmentVariable(Stage));
    }
}