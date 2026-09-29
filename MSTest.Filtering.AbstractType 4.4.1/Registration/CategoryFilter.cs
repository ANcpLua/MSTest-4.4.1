using MSTest.Filtering.AbstractType.Registration;

#pragma warning disable MSTEST0081
[assembly: TestFilterProvider(typeof(CategoryFilter))]
#pragma warning restore MSTEST0081

namespace MSTest.Filtering.AbstractType.Registration;

/// <summary>
/// MSTEST0081 reports a provider that names a filter type which cannot be instantiated: an abstract class as
/// here, a static class, an interface or a ref struct. In a test application every test of the assembly fails
/// with UTA075, for a ref struct with UTA077, so this assembly is a test library that is built and never run.
/// </summary>
public abstract class CategoryFilter : ITestFilter
{
    public abstract TestFilterResult Filter(TestFilterContext context);
}
