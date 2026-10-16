namespace MSTest.Analyzers.Diagnostics;

[TestClass]
public sealed class ProcessGlobalStateRuleTests(TestContext testContext)
{
    [TestMethod]
    [DataRow("Environment.SetEnvironmentVariable(\"MODE\", \"test\");", "EnvironmentVariables")]
    [DataRow("Console.SetOut(TextWriter.Null);", "Console")]
    public async Task Mutation_requires_the_matching_resource_lock(string mutation, string resource)
    {
        var unlocked = await AnalyzerProbe.RunAsync(Source(mutation, "TestMethod", ""),
            new UndeclaredProcessGlobalStateMutationAnalyzer(), testContext.CancellationToken);
        var locked = await AnalyzerProbe.RunAsync(Source(mutation, "TestMethod",
                $"[ResourceLock(WellKnownResources.{resource})]"),
            new UndeclaredProcessGlobalStateMutationAnalyzer(), testContext.CancellationToken);

        Assert.HasCount(1, unlocked);
        Assert.AreEqual("MSTEST0074", unlocked[0].Id);
        Assert.AreEqual(mutation.TrimEnd(';'), (await unlocked[0].Location.SourceTree!.GetTextAsync(testContext.CancellationToken))
            .ToString(unlocked[0].Location.SourceSpan));
        Assert.IsEmpty(locked);
    }

    [TestMethod]
    public async Task Fixture_mutation_requires_a_class_lock()
    {
        const string mutation = "Environment.SetEnvironmentVariable(\"STAGE\", \"test\");";
        var unlocked = await AnalyzerProbe.RunAsync(Source(mutation, "TestInitialize", ""),
            new UndeclaredProcessGlobalStateMutationAnalyzer(), testContext.CancellationToken);
        var locked = await AnalyzerProbe.RunAsync(Source(mutation, "TestInitialize",
                "[ResourceLock(WellKnownResources.EnvironmentVariables)]"),
            new UndeclaredProcessGlobalStateMutationAnalyzer(), testContext.CancellationToken);

        Assert.HasCount(1, unlocked);
        Assert.AreEqual("MSTEST0074", unlocked[0].Id);
        Assert.AreEqual(mutation.TrimEnd(';'), (await unlocked[0].Location.SourceTree!.GetTextAsync(testContext.CancellationToken))
            .ToString(unlocked[0].Location.SourceSpan));
        Assert.IsEmpty(locked);
    }

    private static string Source(string mutation, string attribute, string resourceLock) => $$"""
        using System;
        using System.IO;
        using Microsoft.VisualStudio.TestTools.UnitTesting;
        [assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
        [TestClass]
        {{resourceLock}}
        public class Example
        {
            [{{attribute}}] public void Run() { {{mutation}} }
        }
        """;
}
