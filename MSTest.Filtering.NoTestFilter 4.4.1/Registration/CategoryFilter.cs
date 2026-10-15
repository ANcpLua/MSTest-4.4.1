using MSTest.Filtering.NoTestFilter.Registration;

#pragma warning disable MSTEST0081
[assembly: TestFilterProvider(typeof(CategoryFilter))]
#pragma warning restore MSTEST0081

namespace MSTest.Filtering.NoTestFilter.Registration;

/// <summary>
///     MSTEST0081 reports a provider that names a type which does not implement <see cref="ITestFilter" />, even when
///     it has a method of the same shape. In a test application every test of the assembly fails with UTA076, so
///     this assembly is a test library that is built and never run.
/// </summary>
public sealed class CategoryFilter
{
    // An instance method deliberately mimics ITestFilter without implementing the interface.
    // ReSharper disable once UnusedMember.Global
#pragma warning disable CA1822
    public TestFilterResult Filter(TestFilterContext _)
    {
        return TestFilterResult.Run;
    }
}
#pragma warning restore CA1822
