using MSTest.Filtering.TwoProviders.Registration;

#pragma warning disable MSTEST0081
[assembly: TestFilterProvider(typeof(NightlyFilter))]
[assembly: TestFilterProvider<RetiredFilter>]
#pragma warning restore MSTEST0081

namespace MSTest.Filtering.TwoProviders.Registration;

/// <summary>
/// MSTEST0081 reports each provider of an assembly that registers more than one. The two attribute forms are
/// different types, so the compiler accepts one of each; two providers of the same form are the compiler error
/// CS0579. In a test application every test of the assembly fails with UTA079, so this assembly is a test
/// library that is built and never run.
/// </summary>
public sealed class NightlyFilter : ITestFilter
{
    public TestFilterResult Filter(TestFilterContext context)
        => context.Categories.Contains("Nightly")
            ? TestFilterResult.Skip("Nightly tests run in the nightly build")
            : TestFilterResult.Run;
}

/// <summary>
/// The second filter of the assembly. One filter that takes both decisions replaces the two providers.
/// </summary>
public sealed class RetiredFilter : ITestFilter
{
    public TestFilterResult Filter(TestFilterContext context)
        => context.Categories.Contains("Retired") ? TestFilterResult.Drop : TestFilterResult.Run;
}
