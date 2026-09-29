using MSTest.Filtering.GenericArgument.Registration;

#pragma warning disable MSTEST0081
[assembly: TestFilterProvider<ShardFilter<int>>]
#pragma warning restore MSTEST0081

namespace MSTest.Filtering.GenericArgument.Registration;

/// <summary>
/// MSTEST0081 reports a generic filter type for <c>[assembly: TestFilterProvider&lt;TFilter&gt;]</c> as well: a
/// closed generic type satisfies the constraints of the attribute, and the compiler accepts it. In a test
/// application every test of the assembly fails with UTA074, so this assembly is a test library that is built
/// and never run.
/// </summary>
public sealed class ShardFilter<TShard> : ITestFilter
{
    public TestFilterResult Filter(TestFilterContext context) => TestFilterResult.Run;
}
