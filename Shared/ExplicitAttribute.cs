namespace MSTest.Showcase;

/// <summary>
/// Marks a demonstration that fails, times out or is cancelled on purpose. It is skipped unless the run
/// sets <c>MSTEST_EXPLICIT=1</c>, so an ordinary run stays green and the demonstration is run on request.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class ExplicitAttribute() : ConditionBaseAttribute(ConditionMode.Include)
{
    public const string Variable = "MSTEST_EXPLICIT";

    public override string GroupName => nameof(ExplicitAttribute);

    public override bool IsConditionMet => Environment.GetEnvironmentVariable(Variable) == "1";
}
