using System.Collections.Concurrent;
using MSTest.Filtering.Registration;

[assembly: TestFilterProvider<CategoryFilter>]

namespace MSTest.Filtering.Registration;

/// <summary>
/// <c>[assembly: TestFilterProvider&lt;TFilter&gt;]</c> registers the test filter of the assembly by its type.
/// MSTest creates the filter through its public parameterless constructor and asks it about each test that is
/// about to run: <see cref="TestFilterResult.Run"/> lets the test run, <see cref="TestFilterResult.Skip(string)"/>
/// reports it as skipped with the reason and <see cref="TestFilterResult.Drop"/> leaves it out of the run without
/// a result.
/// </summary>
public sealed class CategoryFilter : ITestFilter
{
    public const string Nightly = "Nightly";
    public const string Retired = "Retired";
    public const string NightlyReason = "Nightly tests run in the nightly build";

    private static readonly ConcurrentDictionary<string, TestFilterAction> Taken = new();

    public static IReadOnlyDictionary<string, TestFilterAction> Decisions => Taken;

    public TestFilterResult Filter(TestFilterContext context)
    {
        TestFilterResult result = context.Categories.Contains(Retired)
            ? TestFilterResult.Drop
            : context.Categories.Contains(Nightly)
                ? TestFilterResult.Skip(NightlyReason)
                : TestFilterResult.Run;

        Taken[context.FullyQualifiedName] = result.Action;
        return result;
    }
}
