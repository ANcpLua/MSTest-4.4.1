using MSTest.Showcase;

namespace MSTest.Reporting.Spans;

/// <summary>
/// A class cleanup method that throws after a test of its class passed, for the spans of a class whose class
/// fixture fails with an exception at the end.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Holds_an_item_before_the_class_fixture_throws
/// passes, a result named "[MSTest.Reporting.Spans.ThrowingClassCleanupTests ClassCleanup]" fails with the
/// exception of ClearTheShelf, and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class ThrowingClassCleanupTests
{
    private static readonly string[] Shelf = ["apple", "pear"];

    [ClassCleanup]
    public static void ClearTheShelf() => throw new InvalidOperationException("The shelf is stuck.");

    [TestMethod]
    public void Holds_an_item_before_the_class_fixture_throws() => Assert.Contains("apple", Shelf);
}
