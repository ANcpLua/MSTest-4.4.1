namespace MSTest.Conditions.ContinuousIntegration;

/// <summary>
///     [CICondition] runs a test in continuous integration only, or with ConditionMode.Exclude skips it there.
///     MSTEST0080 reports a test that starts with a null check on the CI environment variable and then returns or
///     calls Assert.Inconclusive. Its code fix removes the guard and adds the attribute: a null check becomes
///     ConditionMode.Include, a not-null check becomes ConditionMode.Exclude. Where the guard returns, the test is
///     reported as passed although its assertion never ran; the attribute reports it as skipped. The attribute decides
///     differently from the guard: CI counts only when its value parses as boolean true, so CI=false and CI=1 do not
///     count, and it knows the variables of other providers such as TF_BUILD. A test whose condition is met runs as it
///     does without the attribute, so outside continuous integration only the test with ConditionMode.Include fails
///     without its attribute.
/// </summary>
[TestClass]
public sealed class ContinuousIntegrationConditionTests
{
#pragma warning disable MSTEST0080
    [TestMethod]
    public void Guard_returns_early_outside_continuous_integration()
    {
        if (Environment.GetEnvironmentVariable("CI") is null) return;

        Assert.IsNotNull(Environment.GetEnvironmentVariable("CI"));
    }
#pragma warning restore MSTEST0080

    [TestMethod]
    [CICondition(ConditionMode.Include)]
    public void Condition_runs_the_test_in_continuous_integration_only()
    {
        Assert.IsTrue(new CIConditionAttribute(ConditionMode.Include).IsConditionMet);
    }

#pragma warning disable MSTEST0080
    [TestMethod]
    public void Guard_is_inconclusive_in_continuous_integration()
    {
        if (Environment.GetEnvironmentVariable("CI") is not null)
            Assert.Inconclusive("Not supported in continuous integration");

        Assert.IsNull(Environment.GetEnvironmentVariable("CI"));
    }
#pragma warning restore MSTEST0080

    [TestMethod]
    [CICondition(ConditionMode.Exclude)]
    public void Condition_skips_the_test_in_continuous_integration()
    {
        Assert.IsFalse(new CIConditionAttribute(ConditionMode.Exclude).IsConditionMet);
    }
}