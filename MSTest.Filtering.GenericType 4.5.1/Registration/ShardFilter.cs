using MSTest.Filtering.GenericType.Registration;

#pragma warning disable MSTEST0081
[assembly: TestFilterProvider(typeof(ShardFilter<>))]
#pragma warning restore MSTEST0081

namespace MSTest.Filtering.GenericType.Registration;

/// <summary>
///     MSTEST0081 reports a provider that names a generic filter type, open as here or closed as in
///     <c>typeof(ShardFilter&lt;int&gt;)</c>. In a test application every test of the assembly fails with UTA074,
///     so this assembly is a test library that is built and never run.
/// </summary>
// The generic shape is intentionally rejected by MSTEST0081.
// ReSharper disable once UnusedTypeParameter
public sealed class ShardFilter<TShard> : ITestFilter
{
    public TestFilterResult Filter(TestFilterContext context)
    {
        return TestFilterResult.Run;
    }
}