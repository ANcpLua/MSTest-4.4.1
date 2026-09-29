using System.Runtime.InteropServices;

namespace MSTest.Conditions.Architectures;

/// <summary>
/// [ArchitectureCondition] declares the process architectures a test runs on, or with ConditionMode.Exclude the
/// ones it is skipped on. MSTEST0079 reports a test that starts with a guard on
/// RuntimeInformation.ProcessArchitecture and then returns or calls Assert.Inconclusive. Its code fix removes the
/// guard and adds the attribute: an == guard becomes ConditionMode.Exclude, a != guard names the architecture the
/// test runs on. Where the guard returns, the test is reported as passed although its assertion never ran; the
/// attribute reports it as skipped. A test whose condition is met runs as it does without the attribute, so on
/// Arm64 only the test with ConditionMode.Exclude fails without its attribute.
/// </summary>
[TestClass]
public sealed class ArchitectureConditionTests
{
#pragma warning disable MSTEST0079
    [TestMethod]
    public void Guard_returns_early_on_Arm64()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            return;
        }

        Assert.AreNotEqual(Architecture.Arm64, RuntimeInformation.ProcessArchitecture);
    }
#pragma warning restore MSTEST0079

    [TestMethod]
    [ArchitectureCondition(ConditionMode.Exclude, TestArchitectures.Arm64)]
    public void Condition_skips_the_test_on_Arm64()
    {
        Assert.AreNotEqual(Architecture.Arm64, RuntimeInformation.ProcessArchitecture);
    }

#pragma warning disable MSTEST0079
    [TestMethod]
    public void Guard_is_inconclusive_on_other_architectures()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            Assert.Inconclusive("Only for Arm64");
        }

        Assert.AreEqual(Architecture.Arm64, RuntimeInformation.ProcessArchitecture);
    }
#pragma warning restore MSTEST0079

    [TestMethod]
    [ArchitectureCondition(TestArchitectures.Arm64)]
    public void Condition_runs_the_test_on_Arm64_only()
    {
        Assert.AreEqual(Architecture.Arm64, RuntimeInformation.ProcessArchitecture);
    }
}
