using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Testing.Platform.Extensions.TestHostControllers;

namespace MSTest.Platform.Changed.ReleaseLine;

/// <summary>
/// The contract of a test host launcher, the extension that starts the test host in place of the platform,
/// is experimental: <c>ITestHostLauncher</c>, the context it receives and the handle it returns carry the
/// diagnostic <c>TPEXP</c>, and code that names them compiles only where that diagnostic is suppressed.
/// </summary>
[TestClass]
public sealed class TestHostLauncherTests
{
    [TestMethod]
#pragma warning disable TPEXP
    [DataRow(typeof(ITestHostLauncher))]
    [DataRow(typeof(TestHostLaunchContext))]
    [DataRow(typeof(ITestHostHandle))]
#pragma warning restore TPEXP
    public void Contract_is_experimental(Type contract)
    {
        var experimental = contract.GetCustomAttribute<ExperimentalAttribute>();

        Assert.IsNotNull(experimental);
        Assert.AreEqual("TPEXP", experimental.DiagnosticId);
    }
}
