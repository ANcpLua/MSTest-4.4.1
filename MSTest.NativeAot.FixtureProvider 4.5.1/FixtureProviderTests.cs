using System.Runtime.CompilerServices;
using MSTest.SourceGeneration.Library.Fixtures;

#pragma warning disable MSTEST0072
[assembly: AssemblyFixtureProvider(typeof(SharedFixtures))]
#pragma warning restore MSTEST0072

namespace MSTest.NativeAot.FixtureProvider;

/// <summary>
/// MSTEST0072 reports <c>[AssemblyFixtureProvider]</c> in a project with <c>PublishAot</c>. MSTest looks for
/// fixture providers only where the runtime supports dynamic code, and such a project switches dynamic code
/// off: MSTest skips the search, the <c>[AssemblyInitialize]</c> method of the provider never runs and the
/// tests run without it. A Native AOT project declares its assembly fixture in one of its own test classes.
/// </summary>
[TestClass]
public sealed class FixtureProviderTests
{
    [TestMethod]
    public void Dynamic_code_is_switched_off() => Assert.IsFalse(RuntimeFeature.IsDynamicCodeSupported);

    [TestMethod]
    public void Assembly_initializer_of_the_provider_does_not_run() => Assert.IsFalse(SharedFixtures.Initialized);
}
