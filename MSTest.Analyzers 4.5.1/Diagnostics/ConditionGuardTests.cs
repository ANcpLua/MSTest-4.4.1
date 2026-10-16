using Microsoft.CodeAnalysis.Diagnostics;

namespace MSTest.Analyzers.Diagnostics;

[TestClass]
public sealed class ConditionGuardTests(TestContext testContext)
{
    [TestMethod]
    [DataRow("MSTEST0079", "if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) return;", "[ArchitectureCondition(ConditionMode.Exclude, TestArchitectures.Arm64)]")]
    [DataRow("MSTEST0080", "if (Environment.GetEnvironmentVariable(\"CI\") is null) return;", "[CICondition(ConditionMode.Include)]")]
    [DataRow("MSTEST0083", "if (!File.Exists(\"tool\")) return; using var p = Process.Start(\"tool\"); p.WaitForExit();", "[ExecutableCondition(\"tool\")]")]
    public async Task Early_return_guard_is_reported_and_declarative_condition_is_accepted(
        string rule, string guard, string condition)
    {
        var reported = await AnalyzerProbe.RunAsync(Source(guard, ""), Analyzer(rule), testContext.CancellationToken);
        var accepted = await AnalyzerProbe.RunAsync(Source("", condition), Analyzer(rule), testContext.CancellationToken);

        Assert.HasCount(1, reported);
        Assert.AreEqual(rule, reported[0].Id);
        Assert.AreEqual("Example.cs", reported[0].Location.GetLineSpan().Path);
        Assert.IsEmpty(accepted);
    }

    private static DiagnosticAnalyzer Analyzer(string rule) => rule switch
    {
        "MSTEST0079" => new UseArchitectureConditionAttributeInsteadOfRuntimeCheckAnalyzer(),
        "MSTEST0080" => new UseCIConditionAttributeInsteadOfEnvironmentCheckAnalyzer(),
        "MSTEST0083" => new UseExecutableConditionAttributeInsteadOfProcessCheckAnalyzer(),
        _ => throw new ArgumentOutOfRangeException(nameof(rule))
    };

    private static string Source(string guard, string condition) => $$"""
        using System;
        using System.IO;
        using System.Diagnostics;
        using System.Runtime.InteropServices;
        using Microsoft.VisualStudio.TestTools.UnitTesting;
        [TestClass] public class Example
        {
            [TestMethod] {{condition}} public void Test() { {{guard}} Assert.IsTrue(true); }
        }
        """;
}
