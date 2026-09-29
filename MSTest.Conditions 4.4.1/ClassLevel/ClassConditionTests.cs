namespace MSTest.Conditions.ClassLevel;

#pragma warning disable MSTEST0030
#pragma warning disable MSTEST0041
/// <summary>
/// A condition attribute has no effect on a class that lacks [TestClass]: the test method of such a class is not
/// discovered. MSTEST0041 reports the class, and MSTEST0030 reports it as well because it declares a test method.
/// </summary>
[OSCondition(OperatingSystems.Windows)]
public sealed class ConditionWithoutTestClass
{
    [TestMethod]
    public void Is_never_discovered()
    {
        Assert.IsTrue(OperatingSystem.IsWindows());
    }
}
#pragma warning restore MSTEST0041
#pragma warning restore MSTEST0030

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
