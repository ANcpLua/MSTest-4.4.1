# MSTest examples

The examples use **.NET SDK 10.0.401**, **MSTest SDK/framework/adapter 4.5.1** and
**Microsoft.Testing.Platform 2.5.1**. `global.json` selects stable .NET SDKs and the native
MTP test driver. `LegacyBase` deliberately keeps MSTest 3.11.1 for analyzer migration tests;
the other framework references match 4.5.1. Existing project and folder names retain `4.4.1`.

For an ordinary test project, start with `MSTest.Sdk`; it selects the framework, adapter,
platform, executable entry point and default extensions. Add properties and extensions only
for the feature being demonstrated. This is the complete project shape when copied beside
this repository's `global.json`:

```xml
<Project Sdk="MSTest.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

Run the desktop solution with the native MTP driver:

```sh
DOTNET_PROCESSOR_COUNT=2 dotnet test --solution MSTest-4.4.1.slnx --max-parallel-test-modules 1
```

For one project, use `dotnet test --project "path/to/project.csproj"`. MTP options such as
`--report-trx` follow it directly. The processor and module limits above keep this large
showcase's process-heavy scenarios within the local machine's capacity.

Fixtures that deliberately fail, retry after a failure, time out or are cancelled use the
showcase's `[Explicit]` condition. Their corresponding `RunTests` launch the real MTP test
application and assert its results; an ordinary solution run executes those verifiers.
`AttemptRetryRunTests`, for example, checks the final single passed TRX result, the failed
first attempt in CTRF, and the flaky count in the GitHub Actions report.

To examine that retry fixture itself, enable it with a specific filter:

```sh
MSTEST_EXPLICIT=1 DOTNET_PROCESSOR_COUNT=2 dotnet run --project "MSTest.Platform.Changed 4.4.1/MSTest.Platform.Changed 4.4.1.csproj" -- --filter ClassName=MSTest.Platform.Changed.Retries.RecountTests --report-trx
```

`dotnet run` is a [documented MTP entry point](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-run-and-debug).
The .NET 10.0.401 native driver counts superseded in-process retry failures as final failures.
The explicit-fixture arrangement does not repair that SDK bug: it verifies the deliberately
failing scenario through MTP's own executable and structured reports, like the other
failure demonstrations. Ordinary application tests can use MSTest's built-in `[Retry]`;
its retry-history rendering depends on the consuming SDK driver.

Browser-WASM examples use **Node.js** and a published WebAssembly bundle, following the
[upstream browser sample](https://github.com/microsoft/testfx/blob/0bf77e3aedbf86f1fa16220bc951d3b52ba56763/samples/BrowserPlayground/README.md).
`MSTest.Browser.slnx` groups those platform-specific projects separately. Their test code runs
inside the browser-WASM runtime, so their DLLs are not desktop CLR test modules. The projects
keep reflection metadata with `PublishTrimmed=false` and use the prebuilt WASM runtime with
`WasmBuildNative=false`.

Publish and run the runtime example:

```sh
DOTNET_PROCESSOR_COUNT=2 dotnet publish "MSTest.Packaging.Browser 4.4.1/MSTest.Packaging.Browser 4.4.1.csproj" --configuration Debug --output artifacts/browser-runtime
node artifacts/browser-runtime/wwwroot/runtests.mjs --minimum-expected-tests 1
```

Publish and run the browser reporting example:

```sh
DOTNET_PROCESSOR_COUNT=2 dotnet publish "MSTest.Platform.Fixed.Browser 4.4.1/MSTest.Platform.Fixed.Browser 4.4.1.csproj" --configuration Debug --output artifacts/browser-reports
node artifacts/browser-reports/wwwroot/runtests.mjs --minimum-expected-tests 1 --report-trx --report-azdo
```

`BrowserArtifactRunTests` also publishes and runs the reporting fixture under Node during the
desktop solution run, and checks its test count, exit code, TRX path and attachment output.
Reports written by the browser runtime live in its virtual file system. The bundle can also
run in an actual browser: serve its `wwwroot` directory over HTTP and open `index.html`;
test output appears in the browser console, and MTP arguments can be supplied as repeated
`arg` query parameters. `main.mjs` is the browser entry point; `runtests.mjs` forwards Node CLI
arguments and the managed exit code. The SDK's default `dotnet run` launcher is left intact.

`MSTest.Platform.Extension` is a plugin library, so `IsTestingPlatformApplication=false`
correctly overrides MTP's default for that project. `DynamicExtensionRunTests` loads and
exercises it through the executable test host. Ordinary test applications keep the SDK defaults.

Optional Rider/ReSharper inspections, with every severity level included:

```sh
jb inspectcode MSTest-4.4.1.slnx --severity=INFO --format=Sarif --output=artifacts/inspectcode.sarif
```
