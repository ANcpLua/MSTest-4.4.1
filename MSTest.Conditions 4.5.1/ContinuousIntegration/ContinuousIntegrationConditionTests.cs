namespace MSTest.Conditions.ContinuousIntegration;

/// <summary>Declarative conditions determine whether these examples execute.</summary>
[TestClass]
public sealed class ContinuousIntegrationConditionTests
{

    [TestMethod]
    [CICondition(ConditionMode.Include)]
    public void Condition_runs_the_test_in_continuous_integration_only()
    {
        Assert.IsTrue(new CIConditionAttribute(ConditionMode.Include).IsConditionMet);
    }


    [TestMethod]
    [CICondition(ConditionMode.Exclude)]
    public void Condition_skips_the_test_in_continuous_integration()
    {
        Assert.IsFalse(new CIConditionAttribute(ConditionMode.Exclude).IsConditionMet);
    }
}