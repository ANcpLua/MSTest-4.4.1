namespace MSTest.Analyzers.Diagnostics;

[TestClass]
public sealed class DeclarationRuleTests(TestContext testContext)
{
    [TestMethod]
    [DataRow("abstract", 1)]
    [DataRow("sealed", 0)]
    public async Task Abstract_test_classes_are_not_discoverable(string modifier, int expected)
    {
        var source = $$"""
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            [TestClass] public {{modifier}} class Example
            { [TestMethod] public void Test() { } }
            """;
        var diagnostics = await AnalyzerProbe.RunAsync(source,
            new TestClassAttributeShouldNotBeAppliedToAbstractClassAnalyzer(), testContext.CancellationToken);

        Assert.HasCount(expected, diagnostics);
        foreach (var diagnostic in diagnostics) Assert.AreEqual("MSTEST0085", diagnostic.Id);
    }

    [TestMethod]
    [DataRow("same", 1)]
    [DataRow("different", 0)]
    public async Task Explicit_data_row_names_must_be_unique(string secondName, int expected)
    {
        var source = $$"""
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            [TestClass] public class Example
            {
                [TestMethod]
                [DataRow(1, DisplayName = "same")]
                [DataRow(2, DisplayName = "{{secondName}}")]
                public void Test(int value) { }
            }
            """;
        var diagnostics = await AnalyzerProbe.RunAsync(source,
            new DuplicateDataRowDisplayNameAnalyzer(), testContext.CancellationToken);

        Assert.HasCount(expected, diagnostics);
        foreach (var diagnostic in diagnostics) Assert.AreEqual("MSTEST0087", diagnostic.Id);
    }

    [TestMethod]
    [DataRow("full name", 1)]
    [DataRow("", 0)]
    public async Task Full_row_names_conflict_with_argument_only_labels(string fullName, int expected)
    {
        var source = $$"""
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            [TestClass] public class Example
            {
                [TestMethod]
                [DataRow(1, DisplayName = "{{fullName}}", ArgumentsDisplayName = "one")]
                public void Test(int value) { }
            }
            """;
        var diagnostics = await AnalyzerProbe.RunAsync(source,
            new ConflictingDataRowDisplayNamesAnalyzer(), testContext.CancellationToken);

        Assert.HasCount(expected, diagnostics);
        foreach (var diagnostic in diagnostics) Assert.AreEqual("MSTEST0089", diagnostic.Id);
    }

    [TestMethod]
    [DataRow("[OSCondition(OperatingSystems.Windows)]", "sealed", 1)]
    [DataRow("[OSCondition(OperatingSystems.Windows)]", "", 0)]
    [DataRow("", "sealed", 0)]
    public async Task Only_sealed_classes_make_the_repeated_method_condition_redundant(string methodCondition, string modifier, int expected)
    {
        var source = $$"""
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            [TestClass, OSCondition(OperatingSystems.Windows)] public {{modifier}} class Example
            {
                [TestMethod] {{methodCondition}}
                public void Test() { }
            }
            """;
        var diagnostics = await AnalyzerProbe.RunAsync(source,
            new RedundantTestMethodAttributeAnalyzer(), testContext.CancellationToken);

        Assert.HasCount(expected, diagnostics);
        foreach (var diagnostic in diagnostics) Assert.AreEqual("MSTEST0086", diagnostic.Id);
    }
}
