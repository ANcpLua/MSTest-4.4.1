using MSTest.Showcase;

namespace MSTest.Assertions.Differences;

/// <summary>
///     When the strings differ in whitespace that the rendered string cannot show, such as a space, a no-break space or a
///     thin space, the failure of <c>Assert.AreEqual</c> names the difference on both sides between <c>[[</c> and
///     <c>]]</c>, a space as <c>&lt;space&gt;</c>, other whitespace as its escape sequence. Long strings show it in the
///     previews
///     <c>expected near:</c> and <c>actual near:</c>, short ones in the line <c>difference:</c>. A tab, a line feed or a
///     carriage return against one another keeps the caret line, because the rendered strings already show
///     <c>\t</c>, <c>\n</c> and <c>\r</c>.
/// </summary>
// Deliberate comparison failures exercise diagnostic and structured-value reporting; Assert.Fail cannot replace them.
#pragma warning disable MSTEST0025
[TestClass]
public sealed class WhitespaceDifferenceTests
{
    internal const string Statement =
        "SELECT id, customer, total FROM orders WHERE status = 'paid' AND created >= '2026-01-01' ORDER BY";

    [TestMethod]
    [DataRow("\t", @"[[\t]]", DisplayName = "tab")]
    [DataRow("\r\n", @"[[\r\n]]", DisplayName = "line break")]
    [DataRow("\u00A0", @"[[\u00A0]]", DisplayName = "no-break space")]
    [DataRow("\u2009", @"[[\u2009]]", DisplayName = "thin space")]
    public void Preview_names_the_whitespace_on_both_sides(string whitespace, string marker)
    {
        var expected = $"{Statement}{whitespace}created";
        const string actual = $"{Statement} created";

        // Expected varies with the data row; actual is the canonical text whose whitespace is compared.
#pragma warning disable MSTEST0017
        var failure = Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreEqual(expected, actual));
#pragma warning restore MSTEST0017

        Assert.Contains("expected near: \"...", failure.Message);
        Assert.Contains($"ORDER BY{marker}created\"", failure.Message);
        Assert.Contains("actual near:   \"...", failure.Message);
        Assert.Contains("ORDER BY[[<space>]]created\"", failure.Message);
        Assert.Contains("difference:    mismatch marked with [[...]]", failure.Message);
    }

    [TestMethod]
    public void Preview_names_a_space_too_many()
    {
        const string expected = $"{Statement} created";
        const string actual = $"{Statement}  created";

        var failure = Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreEqual(expected, actual));

        Assert.Contains("ORDER BY [[c]]reated\"", failure.Message);
        Assert.Contains("ORDER BY [[<space>]]created\"", failure.Message);
    }

    [TestMethod]
    public void Short_strings_name_the_whitespace_in_the_difference_line()
    {
        const string expected = "total:\t10 EUR";
        const string actual = "total: 10 EUR";

        var failure = Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreEqual(expected, actual));

        Assert.Contains(@"difference: expected [[\t]]; actual [[<space>]]", failure.Message);
    }

    [TestMethod]
    public void Tab_against_line_feed_keeps_the_caret_line()
    {
        const string expected = "total:\t10 EUR";
        const string actual = "total:\n10 EUR";

        var failure = Assert.ThrowsExactly<AssertFailedException>(() => Assert.AreEqual(expected, actual));

        Assert.Contains("difference: -------^", failure.Message);
        Assert.DoesNotContain("[[", failure.Message);
    }
}

/// <summary>
///     Fails on purpose, so that the run prints the preview: <c>expected near:</c> ends with <c>ORDER BY[[\t]]created"</c>
///     ,
///     <c>actual near:</c> with <c>ORDER BY[[&lt;space&gt;]]created"</c>.
/// </summary>
[TestClass]
public sealed class WhitespaceDifferenceShowcase
{
    [TestMethod]
    [Explicit]
    public void Tab_instead_of_a_space()
    {
        const string expected = $"{WhitespaceDifferenceTests.Statement}\tcreated";
        const string actual = $"{WhitespaceDifferenceTests.Statement} created";

        Assert.AreEqual(expected, actual);
    }
}
#pragma warning restore MSTEST0025
