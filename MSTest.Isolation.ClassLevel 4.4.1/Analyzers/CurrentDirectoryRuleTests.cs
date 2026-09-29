namespace MSTest.Isolation.ClassLevel.Analyzers;

/// <summary>
/// MSTEST0075 reports a test that changes the current directory, by Directory.SetCurrentDirectory or by
/// assigning Environment.CurrentDirectory, without declaring the lock for it. Its code fix adds
/// [ResourceLock(WellKnownResources.CurrentDirectory)] to the test.
/// </summary>
[TestClass]
public sealed class CurrentDirectoryRuleTests(TestContext testContext)
{
#pragma warning disable MSTEST0075
    [TestMethod]
    public void Directory_entered_without_a_lock_is_reported()
    {
        string entered = testContext.TestTempDirectory!;
        string previous = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(entered);

        try
        {
            Assert.AreEqual(entered, Directory.GetCurrentDirectory());
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
        }
    }
#pragma warning restore MSTEST0075

    [TestMethod]
    [ResourceLock(WellKnownResources.CurrentDirectory)]
    public void Directory_entered_under_the_lock_is_accepted()
    {
        string entered = testContext.TestTempDirectory!;
        string previous = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(entered);

        try
        {
            Assert.AreEqual(entered, Directory.GetCurrentDirectory());
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
        }
    }

#pragma warning disable MSTEST0075
    [TestMethod]
    public void Directory_assigned_without_a_lock_is_reported()
    {
        string entered = testContext.TestTempDirectory!;
        string previous = Environment.CurrentDirectory;
        Environment.CurrentDirectory = entered;

        try
        {
            Assert.AreEqual(entered, Environment.CurrentDirectory);
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }
    }
#pragma warning restore MSTEST0075

    [TestMethod]
    [ResourceLock(WellKnownResources.CurrentDirectory)]
    public void Directory_assigned_under_the_lock_is_accepted()
    {
        string entered = testContext.TestTempDirectory!;
        string previous = Environment.CurrentDirectory;
        Environment.CurrentDirectory = entered;

        try
        {
            Assert.AreEqual(entered, Environment.CurrentDirectory);
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }
    }
}
