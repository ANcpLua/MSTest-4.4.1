using System.Diagnostics;
using MSTest.Showcase;

namespace MSTest.Conditions.Executables;

/// <summary>Executable conditions resolve a command through PATH or use an explicit path.</summary>
[TestClass]
public sealed class ExecutableConditionTests(TestContext testContext)
{
    [TestMethod]
    [ExecutableCondition("mstest-showcase-intentionally-missing-command")]
    public void Missing_command_does_not_execute_the_test()
        => Assert.Fail("The missing command unexpectedly resolved on PATH.");

    [TestMethod]
    [ExecutableCondition("dotnet")]
    public async Task Available_command_runs_the_test()
    {
        await using var child = ChildProcess.Start(new ProcessStartInfo("dotnet", ["--version"])
        { RedirectStandardOutput = true }, testContext.CancellationToken);
        var version = await child.Process.StandardOutput.ReadToEndAsync(testContext.CancellationToken);
        await child.Process.WaitForExitAsync(testContext.CancellationToken);

        Assert.AreEqual(0, child.Process.ExitCode);
        Assert.IsTrue(Version.TryParse(version.Trim(), out _));
    }
}
