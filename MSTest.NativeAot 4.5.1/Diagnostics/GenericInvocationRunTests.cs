using MSTest.Showcase;

namespace MSTest.NativeAot.Diagnostics;

[TestClass]
public sealed class GenericInvocationRunTests(TestContext testContext)
{
    public static bool IsNativeExecutable => NativeExecutable.IsRunning;

    [TestMethod]
    [MemberCondition(typeof(GenericInvocationRunTests), nameof(IsNativeExecutable))]
    public async Task Native_generic_invocation_reports_the_unavailable_value_type_specialization()
    {
        var run = await RunnerResult.RunAsync($"ClassName={typeof(GenericTestMethodTests).FullName}",
            testContext.CancellationToken);

        Assert.AreEqual(2, run.ExitCode, run.Output);
        Assert.HasCount(2, run.Tests);
        Assert.AreSequenceEqual([typeof(GenericTestMethodTests).FullName], run.Tests.Select(test => test.ClassName).Distinct());
        Assert.HasCount(1, run.Tests.Where(test => test.Outcome == "Failed"));
        Assert.HasCount(1, run.Tests.Where(test => test.Outcome == "Passed"));
        Assert.Contains("Generic_test_is_called_through_reflection", run.Tests.Single(test => test.Outcome == "Failed").Name);
    }
}
