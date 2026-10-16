namespace MSTest.SourceGeneration.GeneratedPath;

/// <summary>
/// The generated registry holds a delegate for the constructor, for the test methods and for the setter of
/// the <see cref="TestContext"/> property of this class, and MSTest calls the class through these delegates.
/// The stack trace of such a member names the registry as its caller and has no frame of
/// <c>System.Reflection</c>. A member that the generator leaves out is called through reflection, as in
/// <see cref="Diagnostics.GenericTestMethodTests"/>.
/// </summary>
[TestClass]
public sealed class GeneratedInvokerTests
{
    private const string Registry = "MSTest.SourceGenerated.MSTestReflectionMetadata";
    private const string Reflection = "System.Reflection";

    private readonly string _constructorCall = Environment.StackTrace;
    private string _setterCall = string.Empty;

    public TestContext TestContext
    {
        get;
        set
        {
            field = value;
            _setterCall = Environment.StackTrace;
        }
    } = null!;

    [TestMethod]
    public void Constructor_is_called_by_the_registry()
    {
        Assert.Contains(Registry, _constructorCall);
        Assert.DoesNotContain(Reflection, _constructorCall);
    }

    [TestMethod]
    public void TestContext_is_set_by_the_registry()
    {
        Assert.Contains(Registry, _setterCall);
        Assert.DoesNotContain(Reflection, _setterCall);
    }

    [TestMethod]
    public void Test_method_is_called_by_the_registry()
    {
        Assert.Contains(Registry, Environment.StackTrace);
        Assert.DoesNotContain(Reflection, Environment.StackTrace);
    }

    [TestMethod]
    public async Task Asynchronous_test_method_is_called_by_the_registry()
    {
        var call = Environment.StackTrace;
        await Task.Yield();

        Assert.Contains(Registry, call);
        Assert.DoesNotContain(Reflection, call);
    }
}
