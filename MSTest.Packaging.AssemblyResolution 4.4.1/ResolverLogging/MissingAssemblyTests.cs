using System.Reflection;

namespace MSTest.Packaging.AssemblyResolution.ResolverLogging;

/// <summary>
/// A logger that throws while MSTest's assembly resolver logs does not change the outcome of assembly resolution.
/// Loading a missing assembly still throws FileNotFoundException, and the adapter writes the failure of the logger
/// once to standard error as "[MSTest.AssemblyResolver] Trace logging failed while resolving '...'". The resolver
/// runs because testconfig.json names a search directory under mstest:assemblyResolution, it logs because
/// Properties/launchSettings.json turns diagnostics on with TESTINGPLATFORM_DIAGNOSTIC, and Program.cs registers
/// the failing logger.
/// </summary>
[TestClass]
public sealed class MissingAssemblyTests
{
    [TestMethod]
    public void Loading_a_missing_assembly_throws_FileNotFoundException()
        => Assert.ThrowsExactly<FileNotFoundException>(
            () => Assembly.Load("MSTest.Packaging.AssemblyResolution.Missing"));
}
