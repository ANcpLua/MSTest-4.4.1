namespace MSTest.Isolation.ClassLevel.Analyzers;

/// <summary>
///     MSTEST0077 reports a constant path that a test hands to a method of File or Directory which creates,
///     writes, moves or deletes. It accepts a path inside TestContext.TestTempDirectory. The relative path of the
///     reported test is resolved against the current directory, so the class declares that it reads the current
///     directory.
/// </summary>
[TestClass]
[ResourceLock(WellKnownResources.CurrentDirectory, Mode = ResourceAccessMode.Read)]
public sealed class SharedPathRuleTests(TestContext testContext)
{
#pragma warning disable MSTEST0077
    [TestMethod]
    public void Constant_path_is_reported()
    {
        File.WriteAllText("report.txt", "reported");

        Assert.AreEqual("reported", File.ReadAllText("report.txt"));

        File.Delete("report.txt");
    }
#pragma warning restore MSTEST0077

    [TestMethod]
    public void Path_in_the_directory_of_the_test_is_accepted()
    {
        var report = Path.Combine(testContext.TestTempDirectory!, "report.txt");

        File.WriteAllText(report, "accepted");

        Assert.AreEqual("accepted", File.ReadAllText(report));
    }
}