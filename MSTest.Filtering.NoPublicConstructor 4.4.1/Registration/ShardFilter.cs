using MSTest.Filtering.NoPublicConstructor.Registration;

#pragma warning disable MSTEST0081
[assembly: TestFilterProvider(typeof(ShardFilter))]
#pragma warning restore MSTEST0081

namespace MSTest.Filtering.NoPublicConstructor.Registration;

/// <summary>
/// MSTEST0081 reports a provider that names a filter class without a public parameterless constructor. In a
/// test application every test of the assembly fails with UTA077, so this assembly is a test library that is
/// built and never run.
/// </summary>
public sealed class ShardFilter(int shard) : ITestFilter
{
    public TestFilterResult Filter(TestFilterContext context)
        => context.FullyQualifiedName.Length % 2 == shard ? TestFilterResult.Run : TestFilterResult.Drop;
}
