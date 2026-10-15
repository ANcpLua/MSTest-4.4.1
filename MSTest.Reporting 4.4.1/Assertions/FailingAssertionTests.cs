using MSTest.Showcase;

namespace MSTest.Reporting.Assertions;

/// <summary>
///     Failures and the values they report next to the failure message. An assertion that compares two values
///     reports the text of both. <c>Assert.Contains</c> on a collection reports the searched item as expected and
///     no actual value, while <c>Assert.Contains</c> on a text reports the searched part as expected and the text
///     as actual value. <c>Assert.Fail</c> and an exception that is no assertion report no values.
///     Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: all six tests fail and the run ends
///     with exit code 2.
/// </summary>
// Deliberate comparison failures exercise diagnostic and structured-value reporting; Assert.Fail cannot replace them.
#pragma warning disable MSTEST0025
[TestClass]
[Explicit]
public sealed class FailingAssertionTests
{
    private static readonly string[] Basket = ["apple", "pear"];

    private const decimal Total = 12.5m;

    [TestMethod]
    public void Compares_two_numbers()
    {
        Assert.AreEqual(15m, Total);
    }

    [TestMethod]
    public void Compares_two_strings()
    {
        Assert.AreEqual("plum", Basket[0]);
    }

    [TestMethod]
    public void Looks_for_an_item()
    {
        Assert.Contains("plum", Basket);
    }

    [TestMethod]
    public void Looks_for_a_part_of_a_text()
    {
        Assert.Contains("plum", "apple pie");
    }

    [TestMethod]
    public void Fails_without_a_comparison()
    {
        Assert.Fail("The basket is not checked.");
    }

    [TestMethod]
    public void Throws_an_exception()
    {
        throw new InvalidOperationException("The basket is locked.");
    }
}
#pragma warning restore MSTEST0025
