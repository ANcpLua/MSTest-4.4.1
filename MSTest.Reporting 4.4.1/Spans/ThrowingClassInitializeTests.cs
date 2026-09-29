using MSTest.Showcase;

namespace MSTest.Reporting.Spans;

/// <summary>
/// A class initialize method that throws, for the spans of a class whose class fixture fails with an exception.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Is_not_reached_after_the_class_fixture
/// fails with the exception of StockTheShelf, and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class ThrowingClassInitializeTests
{
    [ClassInitialize]
    public static void StockTheShelf(TestContext testContext) => throw new InvalidOperationException("The shelf is locked.");

    [TestMethod]
    public void Is_not_reached_after_the_class_fixture() => Assert.Fail("The test runs although its class fixture threw.");
}
