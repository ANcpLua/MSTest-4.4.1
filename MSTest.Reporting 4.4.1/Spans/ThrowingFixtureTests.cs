using MSTest.Showcase;

namespace MSTest.Reporting.Spans;

/// <summary>
/// A test initialize method that throws, for the spans of a test whose test fixture fails with an exception.
/// Expected outcome with MSTEST_EXPLICIT=1 and a filter on this class: Is_not_reached_after_the_exception
/// fails with the exception of OpenTheBasket, and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class ThrowingFixtureTests
{
    [TestInitialize]
    public void OpenTheBasket() => throw new InvalidOperationException("The basket is locked.");

    [TestMethod]
    public void Is_not_reached_after_the_exception() => Assert.Fail("The test runs although its fixture threw.");
}
