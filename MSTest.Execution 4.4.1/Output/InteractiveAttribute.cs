namespace MSTest.Execution.Output;

/// <summary>
/// Marks a demonstration that waits for a line on the standard input. It is skipped unless the run sets
/// <c>MSTEST_INTERACTIVE=1</c>, so an ordinary run does not wait and the demonstration is run on request.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class InteractiveAttribute() : ConditionBaseAttribute(ConditionMode.Include)
{
    public const string Variable = "MSTEST_INTERACTIVE";

    public override string GroupName => nameof(InteractiveAttribute);

    public override bool IsConditionMet => Environment.GetEnvironmentVariable(Variable) == "1";
}
