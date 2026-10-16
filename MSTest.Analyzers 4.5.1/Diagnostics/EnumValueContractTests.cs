namespace MSTest.Analyzers.Diagnostics;

[TestClass]
public sealed class EnumValueContractTests(TestContext testContext)
{
    [TestMethod]
    [DataRow("Assert.AreEqual(-1, (int)DumpType.All);", false)]
    [DataRow("Assert.AreEqual(1, (int)DumpType.Mini);", false)]
    [DataRow("Assert.AreEqual(2, (int)DumpType.Heap);", false)]
    [DataRow("Assert.AreEqual(-1L, (long)Capacity.Unlimited);", false)]
    [DataRow("Assert.AreEqual(1_099_511_627_776L, (long)Capacity.Terabyte);", false)]
    [DataRow("Assert.AreEqual(1, (long)DumpType.Mini);", true)]
    [DataRow("const DumpType selected = DumpType.Heap; Assert.AreEqual(2, (int)selected);", true)]
    public async Task Underlying_enum_values_are_contracts_but_other_constant_assertions_are_reported(
        string body, bool reported)
    {
        var source = $$"""
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            enum DumpType { All = -1, Mini = 1, Heap = 2 }
            enum Capacity : long { Unlimited = -1, Terabyte = 1_099_511_627_776 }
            [TestClass] public class Example
            {
                [TestMethod] public void Test() { {{body}} }
            }
            """;
        var diagnostics = await AnalyzerProbe.RunAsync(source,
            new ReviewAlwaysTrueAssertConditionAnalyzer(), testContext.CancellationToken);

        Assert.HasCount(reported ? 1 : 0, diagnostics);
        if (!reported) return;
        var diagnostic = diagnostics.Single();
        Assert.AreEqual("MSTEST0032", diagnostic.Id);
        Assert.AreEqual("Example.cs", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(body[body.IndexOf("Assert.", StringComparison.Ordinal)..].TrimEnd(';'),
            source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }
}
