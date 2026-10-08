# Public API: MSTest 4.4.1 → 4.5.1

Microsoft.Testing.Platform 2.4.1 → 2.5.1. Source: [microsoft/testfx](https://github.com/microsoft/testfx), tags `v4.4.1` and `v4.5.1`; the changelogs come from `main`.
Taken from `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` of every assembly.

| Surface | Added | Removed |
|---|---:|---:|
| `Adapter/MSTest.Extensions.Hosting/PublicAPI` | 2 | 0 |
| `Adapter/MSTest.TestAdapter/PublicAPI/classicuwp` | 3 | 0 |
| `Adapter/MSTest.TestAdapter/PublicAPI/uwp` | 4 | 0 |
| `Platform/Microsoft.Testing.Extensions.Configuration/PublicAPI` | 2 | 0 |
| `Platform/Microsoft.Testing.Extensions.Hosting/PublicAPI` | 5 | 0 |
| `Platform/Microsoft.Testing.Extensions.OpenTelemetry/PublicAPI` | 5 | 0 |
| `Platform/Microsoft.Testing.Platform/PublicAPI` | 27 | 0 |
| `TestFramework/TestFramework.Windows.UIAutomation/PublicAPI` | 16 | 0 |
| `TestFramework/TestFramework/PublicAPI` | 4 | 0 |

## `Adapter/MSTest.Extensions.Hosting/PublicAPI`

### Added (2)

```text
[MSTESTEXP]Microsoft.Extensions.DependencyInjection.MSTestHostingServiceCollectionExtensions
[MSTESTEXP]static Microsoft.Extensions.DependencyInjection.MSTestHostingServiceCollectionExtensions.AddMSTestTestClassInjection(this Microsoft.Extensions.DependencyInjection.IServiceCollection! services) -> Microsoft.Extensions.DependencyInjection.IServiceCollection!
```

## `Adapter/MSTest.TestAdapter/PublicAPI/classicuwp`

### Added (3)

```text
Microsoft.Testing.Extensions.PackagedAppExtensions
static Microsoft.Testing.Extensions.PackagedAppExtensions.GetTestApplicationArguments(string! activationArguments) -> string![]!
static Microsoft.Testing.Extensions.PackagedAppExtensions.RunTestsAsync(string! activationArguments) -> System.Threading.Tasks.Task<int>!
```

## `Adapter/MSTest.TestAdapter/PublicAPI/uwp`

### Added (4)

```text
Microsoft.VisualStudio.TestTools.UnitTesting.TestApplicationBuilderExtensions
Microsoft.VisualStudio.TestTools.UnitTesting.TestingPlatformBuilderHook
static Microsoft.VisualStudio.TestTools.UnitTesting.TestApplicationBuilderExtensions.AddMSTest(this Microsoft.Testing.Platform.Builder.ITestApplicationBuilder! testApplicationBuilder, System.Func<System.Collections.Generic.IEnumerable<System.Reflection.Assembly!>!>! getTestAssemblies) -> void
static Microsoft.VisualStudio.TestTools.UnitTesting.TestingPlatformBuilderHook.AddExtensions(Microsoft.Testing.Platform.Builder.ITestApplicationBuilder! testApplicationBuilder, string![]! arguments) -> void
```

## `Platform/Microsoft.Testing.Extensions.Configuration/PublicAPI`

### Added (2)

```text
[TPEXP]Microsoft.Testing.Extensions.MicrosoftExtensionsConfigurationBuilderExtensions
[TPEXP]static Microsoft.Testing.Extensions.MicrosoftExtensionsConfigurationBuilderExtensions.AddMicrosoftExtensionsConfigurationSnapshot(this Microsoft.Testing.Platform.Builder.ITestApplicationBuilder! builder, Microsoft.Extensions.Configuration.IConfiguration! configuration, int order = 2) -> Microsoft.Testing.Platform.Builder.ITestApplicationBuilder!
```

## `Platform/Microsoft.Testing.Extensions.Hosting/PublicAPI`

### Added (5)

```text
[TPEXP]Microsoft.Testing.Extensions.ITestingPlatformBuilderConfigurator
[TPEXP]Microsoft.Testing.Extensions.ITestingPlatformBuilderConfigurator.Configure(Microsoft.Testing.Platform.Builder.ITestApplicationBuilder! testApplicationBuilder) -> void
[TPEXP]Microsoft.Testing.Extensions.MicrosoftExtensionsHostingExtensions
[TPEXP]static Microsoft.Testing.Extensions.MicrosoftExtensionsHostingExtensions.RunTestingPlatformAsync(this Microsoft.Extensions.Hosting.IHost! host, string![]! args, System.Action<Microsoft.Testing.Platform.Builder.ITestApplicationBuilder!>! configure, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) -> System.Threading.Tasks.Task<int>!
[TPEXP]static Microsoft.Testing.Extensions.MicrosoftExtensionsHostingExtensions.ShouldBypassApplicationHost(string![]! args) -> bool
```

## `Platform/Microsoft.Testing.Extensions.OpenTelemetry/PublicAPI`

### Added (5)

```text
Microsoft.Testing.Extensions.OpenTelemetry.TestingPlatformBuilderHook
static Microsoft.Testing.Extensions.OpenTelemetry.TestingPlatformBuilderHook.AddExtensions(Microsoft.Testing.Platform.Builder.ITestApplicationBuilder! testApplicationBuilder, string![]! _) -> void
static Microsoft.Testing.Extensions.OpenTelemetryProviderExtensions.AddTestingPlatformCIResource(this OpenTelemetry.Resources.ResourceBuilder! builder) -> OpenTelemetry.Resources.ResourceBuilder!
static Microsoft.Testing.Extensions.OpenTelemetryProviderExtensions.AddTestingPlatformDiagnostics(this Microsoft.Testing.Platform.Builder.ITestApplicationBuilder! builder) -> void
static Microsoft.Testing.Extensions.OpenTelemetryProviderExtensions.AddTestingPlatformTestResource(this OpenTelemetry.Resources.ResourceBuilder! builder) -> OpenTelemetry.Resources.ResourceBuilder!
```

## `Platform/Microsoft.Testing.Platform/PublicAPI`

### Added (27)

```text
Microsoft.Testing.Platform.Builder.TestApplicationOptions.CancellationToken.get -> System.Threading.CancellationToken
Microsoft.Testing.Platform.Builder.TestApplicationOptions.CancellationToken.set -> void
[TPEXP]Microsoft.Testing.Platform.Extensions.TestFramework.ExecuteRequestContext.StartTestExecutionAsync(Microsoft.Testing.Platform.Extensions.Messages.IDataProducer! dataProducer, Microsoft.Testing.Platform.Extensions.Messages.TestNodeUpdateMessage! inProgressMessage) -> System.Threading.Tasks.Task<Microsoft.Testing.Platform.Extensions.TestFramework.TestExecution!>!
[TPEXP]Microsoft.Testing.Platform.Extensions.TestFramework.TestExecution
[TPEXP]Microsoft.Testing.Platform.Extensions.TestFramework.TestExecution.CompleteAsync(System.Collections.Generic.IReadOnlyList<Microsoft.Testing.Platform.Extensions.Messages.TestNodeUpdateMessage!>! resultMessages, System.DateTimeOffset executionEndTime) -> System.Threading.Tasks.Task!
[TPEXP]Microsoft.Testing.Platform.Extensions.TestFramework.TestExecution.Dispose() -> void
[TPEXP]Microsoft.Testing.Platform.Extensions.TestFramework.TestExecution.Run(System.Action! callback) -> void
[TPEXP]Microsoft.Testing.Platform.Extensions.TestFramework.TestExecution.Run<T>(System.Func<T>! callback) -> T
[TPEXP]Microsoft.Testing.Platform.Extensions.TestFramework.TestExecution.RunAsync(System.Func<System.Threading.Tasks.Task!>! callback) -> System.Threading.Tasks.Task!
[TPEXP]Microsoft.Testing.Platform.Extensions.TestFramework.TestExecution.RunAsync<T>(System.Func<System.Threading.Tasks.Task<T>!>! callback) -> System.Threading.Tasks.Task<T>!
[TPEXP]Microsoft.Testing.Platform.Extensions.TestHostControllers.ILocalTestHostHandle
[TPEXP]Microsoft.Testing.Platform.Extensions.TestHostControllers.ILocalTestHostHandle.ProcessId.get -> int
[TPEXP]Microsoft.Testing.Platform.Extensions.TestHostControllers.ITestHostHandleExitCodePolicy
[TPEXP]Microsoft.Testing.Platform.Extensions.TestHostControllers.ITestHostHandleExitCodePolicy.IsExitCodeAuthoritative.get -> bool
[TPEXP]Microsoft.Testing.Platform.Extensions.TestHostOrchestrator.ITestHostExecutionOrchestratorMiddleware
[TPEXP]Microsoft.Testing.Platform.Extensions.TestHostOrchestrator.ITestHostExecutionOrchestratorMiddleware.OrchestrateTestHostExecutionAsync(System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<int>!>! next, System.Threading.CancellationToken cancellationToken) -> System.Threading.Tasks.Task<int>!
[TPEXP]Microsoft.Testing.Platform.Logging.IDiagnosticLoggingInformation
[TPEXP]Microsoft.Testing.Platform.Logging.IDiagnosticLoggingInformation.LogFile.get -> System.IO.FileInfo!
[TPEXP]Microsoft.Testing.Platform.Logging.IDiagnosticLoggingInformation.LogLevel.get -> Microsoft.Testing.Platform.Logging.LogLevel
[TPEXP]Microsoft.Testing.Platform.Logging.IDiagnosticLoggingInformation.SynchronousWrite.get -> bool
[TPEXP]Microsoft.Testing.Platform.TestHostOrchestrator.ITestHostExecutionOrchestratorMiddlewareManager
[TPEXP]Microsoft.Testing.Platform.TestHostOrchestrator.ITestHostExecutionOrchestratorMiddlewareManager.AddTestHostExecutionOrchestratorMiddleware(System.Func<System.IServiceProvider!, Microsoft.Testing.Platform.Extensions.TestHostOrchestrator.ITestHostExecutionOrchestratorMiddleware!>! factory) -> void
[TPEXP]Microsoft.Testing.Platform.TestHostOrchestrator.TestHostOrchestratorManagerExtensions
[TPEXP]static Microsoft.Testing.Platform.Services.ServiceProviderExtensions.GetDiagnosticLoggingInformation(this System.IServiceProvider! serviceProvider) -> Microsoft.Testing.Platform.Logging.IDiagnosticLoggingInformation?
[TPEXP]static Microsoft.Testing.Platform.TestHostOrchestrator.TestHostOrchestratorManagerExtensions.AddTestHostExecutionOrchestratorMiddleware(this Microsoft.Testing.Platform.TestHostOrchestrator.ITestHostOrchestratorManager! manager, System.Func<System.IServiceProvider!, Microsoft.Testing.Platform.Extensions.TestHostOrchestrator.ITestHostExecutionOrchestratorMiddleware!>! factory) -> void
static Microsoft.Testing.Platform.Services.ServiceProviderExtensions.GetPlatformService<TService>(this System.IServiceProvider! provider) -> TService?
static Microsoft.Testing.Platform.Services.ServiceProviderExtensions.GetRequiredPlatformService<TService>(this System.IServiceProvider! provider) -> TService
```

## `TestFramework/TestFramework.Windows.UIAutomation/PublicAPI`

### Added (16)

```text
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest.AppProcess.get -> System.Diagnostics.Process!
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest.ApplicationSetup() -> void
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest.ApplicationTest() -> void
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest.Dispose() -> void
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest.TestContext.get -> Microsoft.VisualStudio.TestTools.UnitTesting.TestContext!
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest.TestContext.set -> void
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.WindowTest
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.WindowTest.MainWindow.get -> System.Windows.Automation.AutomationElement!
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.WindowTest.WindowSetup() -> void
Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.WindowTest.WindowTest() -> void
abstract Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest.CreateProcessStartInfo() -> System.Diagnostics.ProcessStartInfo!
virtual Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest.ApplicationShutdownTimeout.get -> System.TimeSpan
virtual Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.ApplicationTest.StopApplication(System.Diagnostics.Process! applicationProcess) -> void
virtual Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.WindowTest.FindWindow(System.Diagnostics.Process! applicationProcess) -> System.Windows.Automation.AutomationElement?
virtual Microsoft.VisualStudio.TestTools.UnitTesting.Windows.UIAutomation.WindowTest.WindowDiscoveryTimeout.get -> System.TimeSpan
```

## `TestFramework/TestFramework/PublicAPI`

### Added (4)

```text
Microsoft.VisualStudio.TestTools.UnitTesting.DataRowAttribute.ArgumentsDisplayName.get -> string?
Microsoft.VisualStudio.TestTools.UnitTesting.DataRowAttribute.ArgumentsDisplayName.set -> void
Microsoft.VisualStudio.TestTools.UnitTesting.TestDataRow<T>.ArgumentsDisplayName.get -> string?
Microsoft.VisualStudio.TestTools.UnitTesting.TestDataRow<T>.ArgumentsDisplayName.set -> void
```
