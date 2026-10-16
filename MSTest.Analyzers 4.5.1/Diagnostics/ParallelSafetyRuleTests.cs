using Microsoft.CodeAnalysis.Diagnostics;

namespace MSTest.Analyzers.Diagnostics;

[TestClass]
public sealed class ParallelSafetyRuleTests(TestContext testContext)
{
    [TestMethod]
    [DataRow("MSTEST0075", "Directory.SetCurrentDirectory(\"path\");", "CurrentDirectory")]
    [DataRow("MSTEST0075", "Environment.CurrentDirectory = \"path\";", "CurrentDirectory")]
    [DataRow("MSTEST0076", "CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;", "culture")]
    [DataRow("MSTEST0076", "CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;", "culture")]
    public async Task Shared_process_state_requires_serialization(string rule, string operation, string key)
    {
        var unlocked = await AnalyzerProbe.RunAsync(Source(operation, ""), Analyzer(rule), testContext.CancellationToken);
        var locked = await AnalyzerProbe.RunAsync(Source(operation, key == "CurrentDirectory"
                ? "[ResourceLock(WellKnownResources.CurrentDirectory)]"
                : $"[ResourceLock(\"{key}\")]"),
            Analyzer(rule), testContext.CancellationToken);
        var sequential = await AnalyzerProbe.RunAsync(Source(operation, "[DoNotParallelize]"),
            Analyzer(rule), testContext.CancellationToken);

        Assert.HasCount(1, unlocked);
        Assert.AreEqual(rule, unlocked[0].Id);
        Assert.IsEmpty(locked);
        Assert.IsEmpty(sequential);
    }

    [TestMethod]
    public async Task Async_local_culture_does_not_mutate_the_process_default()
    {
        var diagnostics = await AnalyzerProbe.RunAsync(
            Source("CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;", ""),
            new CultureMutationUnderParallelizationAnalyzer(), testContext.CancellationToken);

        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task Shared_literal_path_is_reported_but_per_test_directory_is_accepted()
    {
        var shared = await AnalyzerProbe.RunAsync(Source("File.WriteAllText(\"report.txt\", \"result\");", ""),
            new SharedFileSystemPathInTestAnalyzer(), testContext.CancellationToken);
        var isolated = await AnalyzerProbe.RunAsync(
            Source("File.WriteAllText(Path.Combine(TestContext.TestTempDirectory!, \"report.txt\"), \"result\");", ""),
            new SharedFileSystemPathInTestAnalyzer(), testContext.CancellationToken);

        Assert.HasCount(1, shared);
        Assert.AreEqual("MSTEST0077", shared[0].Id);
        Assert.IsEmpty(isolated);
    }

    private static DiagnosticAnalyzer Analyzer(string rule) => rule switch
    {
        "MSTEST0075" => new CurrentDirectoryMutationUnderParallelizationAnalyzer(),
        "MSTEST0076" => new CultureMutationUnderParallelizationAnalyzer(),
        _ => throw new ArgumentOutOfRangeException(nameof(rule))
    };

    private static string Source(string operation, string isolation) => $$"""
        using System;
        using System.IO;
        using System.Globalization;
        using Microsoft.VisualStudio.TestTools.UnitTesting;
        [assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
        [TestClass] public class Example
        {
            public TestContext TestContext { get; set; } = null!;
            [TestMethod] {{isolation}} public void Test() { {{operation}} }
        }
        """;
}
