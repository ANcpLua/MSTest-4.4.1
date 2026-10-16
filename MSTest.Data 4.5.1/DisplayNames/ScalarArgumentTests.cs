namespace MSTest.Data.DisplayNames;

/// <summary>
/// A data row without a display name of its own is named after the test and its arguments: a string in
/// double quotes, a <see cref="char"/> in single quotes, <see langword="null"/> as <c>null</c> and any other
/// value as its <c>ToString</c> writes it. Quotes in string and character arguments are escaped.
/// </summary>
[TestClass]
public sealed class ScalarArgumentTests(TestContext testContext)
{
    [TestMethod]
    [DataRow("pen", 'A', 3, true, DayOfWeek.Monday, null)]
    public void Names_the_row_after_its_arguments(
        string item, char grade, int quantity, bool express, DayOfWeek delivery, string? note)
        => Assert.AreEqual(
            $"""{nameof(Names_the_row_after_its_arguments)} ("pen",'A',3,True,Monday,null)""",
            testContext.TestDisplayName);

    [TestMethod]
    [DataRow("6\" nail", '\'')]
    public void Escapes_quotes_in_text(string item, char unit)
        => Assert.AreEqual(
            $"""{nameof(Escapes_quotes_in_text)} ("6\" nail",'\'')""",
            testContext.TestDisplayName);
    [TestMethod]
    [DataRow("line\nnext\tend")]
    public void Escapes_control_characters(string value)
        => Assert.AreEqual(
            "Escapes_control_characters (\"line\\nnext\\tend\")",
            testContext.TestDisplayName);
}
