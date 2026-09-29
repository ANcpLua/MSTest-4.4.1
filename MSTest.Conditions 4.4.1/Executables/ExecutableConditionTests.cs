using System.Diagnostics;

namespace MSTest.Conditions.Executables;

/// <summary>
/// [ExecutableCondition] runs a test only where the named executable is available. MSTEST0083 reports a test that
/// starts with a File.Exists guard, returns or calls Assert.Inconclusive when the file is missing, and later passes
/// the same path to Process.Start. Its code fix removes the guard and adds the attribute. Where the guard returns,
/// the test is reported as passed although the tool never ran; the attribute reports it as skipped. A path with a
/// directory separator is checked as it is, a bare name is resolved against PATH. A test whose condition is met
/// runs as it does without the attribute, so only the test for the missing tool fails without its attribute.
/// </summary>
[TestClass]
public sealed class ExecutableConditionTests
{
#pragma warning disable MSTEST0083
    [TestMethod]
    public void Guard_returns_early_without_the_tool()
    {
        if (!File.Exists("/opt/mstest-showcase/missing-tool"))
        {
            return;
        }

        using Process process = Process.Start("/opt/mstest-showcase/missing-tool");
        process.WaitForExit();

        Assert.AreEqual(0, process.ExitCode);
    }
#pragma warning restore MSTEST0083

    [TestMethod]
    [ExecutableCondition("/opt/mstest-showcase/missing-tool")]
    public void Condition_skips_the_test_without_the_tool()
    {
        using Process process = Process.Start("/opt/mstest-showcase/missing-tool");
        process.WaitForExit();

        Assert.AreEqual(0, process.ExitCode);
    }

#pragma warning disable MSTEST0083
    [TestMethod]
    public void Guard_is_inconclusive_without_the_tool()
    {
        if (!File.Exists("/usr/bin/true"))
        {
            Assert.Inconclusive("The tool is required");
        }

        using Process process = Process.Start("/usr/bin/true");
        process.WaitForExit();

        Assert.AreEqual(0, process.ExitCode);
    }
#pragma warning restore MSTEST0083

    [TestMethod]
    [ExecutableCondition("/usr/bin/true")]
    public void Condition_runs_the_test_with_the_tool()
    {
        using Process process = Process.Start("/usr/bin/true");
        process.WaitForExit();

        Assert.AreEqual(0, process.ExitCode);
    }
}
