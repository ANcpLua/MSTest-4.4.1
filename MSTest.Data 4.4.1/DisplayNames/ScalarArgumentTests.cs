namespace MSTest.Data.DisplayNames;

/// <summary>
/// A data row without a display name of its own is named after the test and its arguments: a string in
/// double quotes, a <see cref="char"/> in single quotes, <see langword="null"/> as <c>null</c> and any other
/// value as its <c>ToString</c> writes it. Text is copied as it is, without escaping.
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
    public void Copies_text_without_escaping(string item, char unit)
        => Assert.AreEqual(
            $"""{nameof(Copies_text_without_escaping)} ("6" nail",''')""",
            testContext.TestDisplayName);
}
