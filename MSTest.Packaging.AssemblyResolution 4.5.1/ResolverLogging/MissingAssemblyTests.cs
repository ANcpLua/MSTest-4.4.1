using System.Reflection;

namespace MSTest.Packaging.AssemblyResolution.ResolverLogging;

/// <summary>
/// A logger that throws while MSTest's assembly resolver logs does not change the outcome of assembly resolution.
/// Loading a missing assembly still throws FileNotFoundException, and the logger failure counter proves that the throwing logger was actually reached. The resolver
/// runs because testconfig.json names a search directory under mstest:assemblyResolution, it logs because
/// Properties/launchSettings.json turns diagnostics on with TESTINGPLATFORM_DIAGNOSTIC, and Program.cs registers
/// the failing logger.
/// </summary>
[TestClass]
public sealed class MissingAssemblyTests
{
    [TestMethod]
    public void Loading_a_missing_assembly_preserves_the_exception_when_the_resolver_logger_throws()
    {
        var before = FailingResolverLoggerProvider.FailureCount;
        var failure = Assert.ThrowsExactly<FileNotFoundException>(
            () => Assembly.Load("MSTest.Packaging.AssemblyResolution.Missing"));

        Assert.AreEqual("MSTest.Packaging.AssemblyResolution.Missing, Culture=neutral, PublicKeyToken=null", failure.FileName);
        Assert.IsGreaterThan(before, FailingResolverLoggerProvider.FailureCount);
    }
}
