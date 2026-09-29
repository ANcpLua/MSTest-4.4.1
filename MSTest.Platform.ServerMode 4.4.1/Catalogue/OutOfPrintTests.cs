using MSTest.Showcase;

namespace MSTest.Platform.ServerMode.Catalogue;

/// <summary>
/// A test that fails, for a client that reads the failure from the node of the test. Expected outcome with
/// MSTEST_EXPLICIT=1 and a filter on this class: the test fails and the run ends with exit code 2.
/// </summary>
[TestClass]
[Explicit]
public sealed class OutOfPrintTests
{
    [TestMethod]
    public void Prices_a_book_that_is_out_of_print() => Assert.Fail("The book has no list price.");
}
