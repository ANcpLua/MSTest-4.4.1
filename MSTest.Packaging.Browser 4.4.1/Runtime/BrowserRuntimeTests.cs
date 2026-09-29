using System.Runtime.InteropServices;

namespace MSTest.Packaging.Browser.Runtime;

/// <summary>
/// A project on the Microsoft.NET.Sdk.WebAssembly SDK with EnableMSTestRunner and the MSTest.TestAdapter and
/// MSTest.TestFramework packages is a Microsoft.Testing.Platform test application for browser-wasm. wwwroot/main.mjs
/// boots the built app through _framework/dotnet.js and passes the arguments on, and dotnet run starts it under the
/// JavaScript host bun. The tests run inside the browser-wasm runtime.
/// </summary>
[TestClass]
public sealed class BrowserRuntimeTests
{
    [TestMethod]
    public void Runs_in_the_browser_wasm_runtime()
        => Assert.AreEqual("browser-wasm", RuntimeInformation.RuntimeIdentifier);
}
