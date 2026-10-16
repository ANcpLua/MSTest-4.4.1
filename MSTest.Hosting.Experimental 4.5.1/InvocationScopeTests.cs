using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace MSTest.Hosting.Experimental;

[TestClass]
public sealed class InvocationScopeTests(
    InvocationState state, ScopeJournal journal, IConfiguration configuration,
    IServiceProvider services, TestContext testContext)
{
    [TestInitialize]
    public void InitializeInvocation()
    {
        Assert.AreEqual(0, state.Value);
        Assert.IsTrue(journal.Executions.TryAdd(state.Id, testContext.TestDisplayName!));
        state.Value = 10;
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(5)]
    public void Data_rows_receive_independent_scopes(int increment)
    {
        Assert.AreEqual(10, state.Value);
        Assert.AreSame(state, services.GetRequiredService<InvocationState>());
        state.Value += increment;
        Assert.AreEqual(10 + increment, state.Value);
        Assert.AreEqual("hello from the host", configuration["Greeting"]);
    }

    [TestCleanup]
    public void CleanupRetainsTheInvocationState()
    {
        Assert.IsTrue(state.Value is 12 or 15);
    }
}
