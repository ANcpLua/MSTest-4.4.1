namespace MSTest.Conditions.ClassLevel;

/// <summary>
/// The code fix for MSTEST0041 adds [TestClass] below the condition attribute. The condition then decides for
/// every test of the class, which is reported as skipped outside Windows.
/// </summary>
[OSCondition(OperatingSystems.Windows)]
[TestClass]
public sealed class ConditionOnTestClass
{
    [TestMethod]
    public void Runs_on_Windows_only()
    {
        Assert.IsTrue(OperatingSystem.IsWindows());
    }
}
