using System.Runtime.InteropServices;

namespace MSTest.Conditions.Architectures;

/// <summary>Declarative conditions determine whether these examples execute.</summary>
[TestClass]
public sealed class ArchitectureConditionTests
{

    [TestMethod]
    [ArchitectureCondition(ConditionMode.Exclude, TestArchitectures.Arm64)]
    public void Condition_skips_the_test_on_Arm64()
    {
        Assert.AreNotEqual(Architecture.Arm64, RuntimeInformation.ProcessArchitecture);
    }


    [TestMethod]
    [ArchitectureCondition(TestArchitectures.Arm64)]
    public void Condition_runs_the_test_on_Arm64_only()
    {
        Assert.AreEqual(Architecture.Arm64, RuntimeInformation.ProcessArchitecture);
    }
}
