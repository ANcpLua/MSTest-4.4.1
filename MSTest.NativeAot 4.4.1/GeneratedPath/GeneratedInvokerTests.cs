using System.Runtime.CompilerServices;

namespace MSTest.NativeAot.GeneratedPath;

/// <summary>
/// <c>PublishAot</c> in a project of MSTest.Sdk adds the source generator without a further property and
/// switches dynamic code off, under <c>dotnet run</c> as well as in the native executable. MSTest calls the
/// constructor and the test through the delegates of the generated registry
/// <c>MSTest.SourceGenerated.MSTestReflectionMetadata</c>, which the stack trace names in both hosts.
/// </summary>
[TestClass]
public sealed class GeneratedInvokerTests
{
    private const string Registry = "MSTest.SourceGenerated.MSTestReflectionMetadata";
    private const string Reflection = "System.Reflection";

    private readonly string _constructorCall = Environment.StackTrace;

    [TestMethod]
    public void Dynamic_code_is_switched_off() => Assert.IsFalse(RuntimeFeature.IsDynamicCodeSupported);

    [TestMethod]
    public void Constructor_is_called_by_the_registry()
    {
        Assert.Contains(Registry, _constructorCall);
        Assert.DoesNotContain(Reflection, _constructorCall);
    }

    [TestMethod]
    public void Test_method_is_called_by_the_registry()
    {
        Assert.Contains(Registry, Environment.StackTrace);
        Assert.DoesNotContain(Reflection, Environment.StackTrace);
    }
}
