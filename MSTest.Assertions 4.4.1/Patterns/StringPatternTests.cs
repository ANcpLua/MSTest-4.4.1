using System.Globalization;
using System.Text.RegularExpressions;

namespace MSTest.Assertions.Patterns;

/// <summary>
/// <c>Assert.MatchesRegex</c> and <c>Assert.DoesNotMatchRegex</c> build the regular expression of a pattern string once
/// and reuse it for the same pattern under the same culture.
/// </summary>
[TestClass]
public sealed class StringPatternTests
{
    [TestMethod]
    public void MatchesRegex_reuses_the_regular_expression_of_a_pattern()
    {
        const string Pattern = @"^ORD-\d{6}$";
        Assert.MatchesRegex(Pattern, "ORD-000001");

        long repetition = AllocatedBy(() => Assert.MatchesRegex(Pattern, "ORD-000002"));

        Assert.IsLessThan(Construction(Pattern), repetition);
    }

    [TestMethod]
    public void DoesNotMatchRegex_reuses_the_regular_expression_of_a_pattern()
    {
        const string Pattern = @"^INV-\d{6}$";
        Assert.DoesNotMatchRegex(Pattern, "ORD-000001");

        long repetition = AllocatedBy(() => Assert.DoesNotMatchRegex(Pattern, "ORD-000002"));

        Assert.IsLessThan(Construction(Pattern), repetition);
    }

    // In Turkish the lower case of "I" is the dotless i and not "i", so the same pattern gives another answer.
    [TestMethod]
    public void The_current_culture_decides_a_case_insensitive_match()
    {
        const string Pattern = "(?i)^i$";
        CultureInfo culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.MatchesRegex(Pattern, "I");

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.DoesNotMatchRegex(Pattern, "I");
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    // What building the regular expression allocates, measured on the second construction,
    // which allocates less than the first one.
    private static long Construction(string pattern)
    {
        _ = new Regex(pattern);
        return AllocatedBy(() => _ = new Regex(pattern));
    }

    private static long AllocatedBy(Action action)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}
