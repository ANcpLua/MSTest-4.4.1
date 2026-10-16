using System.Globalization;
using System.Text.RegularExpressions;

namespace MSTest.Assertions.Patterns;

/// <summary>
///     <c>Assert.MatchesRegex</c> and <c>Assert.DoesNotMatchRegex</c> reuse the regular expression for a pattern
///     under the same culture while it remains in the bounded cache. Patterns longer than 512 characters
///     are constructed again rather than retained.
/// </summary>
[TestClass]
public sealed class StringPatternTests
{
    [TestMethod]
    public void MatchesRegex_reuses_the_regular_expression_of_a_pattern()
    {
        const string pattern = @"^ORD-\d{6}$";
        Assert.MatchesRegex(pattern, "ORD-000001");

        var repetition = AllocatedBy(() => Assert.MatchesRegex(pattern, "ORD-000002"));

        Assert.IsLessThan(Construction(pattern), repetition);
    }

    [TestMethod]
    public void DoesNotMatchRegex_reuses_the_regular_expression_of_a_pattern()
    {
        const string pattern = @"^INV-\d{6}$";
        Assert.DoesNotMatchRegex(pattern, "ORD-000001");

        var repetition = AllocatedBy(() => Assert.DoesNotMatchRegex(pattern, "ORD-000002"));

        Assert.IsLessThan(Construction(pattern), repetition);
    }

    // In Turkish the lower case of "I" is the dotless i and not "i", so the same pattern gives another answer.
    [TestMethod]
    public void The_current_culture_decides_a_case_insensitive_match()
    {
        const string pattern = "(?i)^i$";
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.MatchesRegex(pattern, "I");

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.DoesNotMatchRegex(pattern, "I");
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
        var before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}