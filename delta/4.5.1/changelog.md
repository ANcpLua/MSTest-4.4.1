# Changelog: MSTest 4.4.1 → 4.5.1

Microsoft.Testing.Platform 2.4.1 → 2.5.1. Source: [microsoft/testfx](https://github.com/microsoft/testfx), tags `v4.4.1` and `v4.5.1`; the changelogs come from `main`.

# MSTest

## <a name="4.5.1" />[4.5.1] - 2026-10-07

See full log [of v4.5.0...v4.5.1](https://github.com/microsoft/testfx/compare/v4.5.0...v4.5.1)

There was a problem with the release infrastructure preventing us from shipping `4.5.0`.

## <a name="4.5.0" />[4.5.0] - Unreleased

See full log [of v4.4.1...v4.5.0](https://github.com/microsoft/testfx/compare/v4.4.1...v4.5.0)

### Added

* Add the preview `MSTest.Windows.UIAutomation` package for MSTest-managed application lifecycle and UIA2 window access in unpackaged Win32, WinForms and WPF tests, with opt-in `MSTest.Sdk` integration for Microsoft.Testing.Platform and VSTest, by @Evangelink in [#10862](https://github.com/microsoft/testfx/pull/10862)
* Run classic UWP, modern UWP, and AppContainer-configured WinUI test applications end to end through Microsoft.Testing.Platform using the MSTest.Sdk app-model sidecar controller, with AUMID activation, exact package-SID IPC authorization, UI-thread tests, TRX, HangDump, Retry, and encrypted activation payloads, by @Evangelink in [#11484](https://github.com/microsoft/testfx/pull/11484)
* Ship the MSTest adapter's Microsoft.Testing.Platform integration and required runtime assets for classic and modern UWP targets while retaining VSTest routing for true UWP/AppContainer `MSTest.Sdk` applications, by @Evangelink in [#11118](https://github.com/microsoft/testfx/pull/11118)
* Add opt-in assertion failure diagnostics through `CaptureAssertionFailureDiagnostics` / `mstest:execution:captureAssertionFailureDiagnostics`, producing bounded per-test artifacts with assertion values, source frames, concurrent tests and process state for supported VSTest and Microsoft.Testing.Platform runs, by @Evangelink in [#11244](https://github.com/microsoft/testfx/pull/11244)
* Add the opt-in prerelease `MSTest.Extensions.Hosting` package so MSTest test classes can be constructed from a caller-owned Microsoft.Extensions.Hosting service provider with one dependency-injection scope per test invocation, data row and retry, by @Evangelink in [#11635](https://github.com/microsoft/testfx/pull/11635)
* Add MSTEST0084 and a code fix to align MSTest `OSCondition` attributes with `SupportedOSPlatformAttribute` and `UnsupportedOSPlatformAttribute`, including safe handling of versioned platform constraints, by @Evangelink in [#11044](https://github.com/microsoft/testfx/pull/11044) and [#11302](https://github.com/microsoft/testfx/pull/11302)
* Add MSTEST0085 to report `[TestClass]` and derived attributes on abstract classes that MSTest cannot discover directly, by @Evangelink in [#11240](https://github.com/microsoft/testfx/pull/11240)
* Add MSTEST0086 and a code fix to remove method-level MSTest attributes whose effective behavior is already supplied by the containing test class, by @Evangelink in [#11267](https://github.com/microsoft/testfx/pull/11267)
* Add MSTEST0087 to report duplicate explicit `DataRow` display names within the same test method, by @Evangelink in [#11409](https://github.com/microsoft/testfx/pull/11409)
* Add MSTEST0088 to reject hosted test-class injection with Native AOT, browser WebAssembly, AOT compilation or MSTest source generation, and update MSTEST0063 to validate dependency-injected test-class constructors, by @Evangelink in [#11635](https://github.com/microsoft/testfx/pull/11635)
* Add opt-in `ArgumentsDisplayName` properties to `DataRowAttribute` and `TestDataRow<T>` so a data row can label only its arguments while retaining the test method's display name. Existing full `DisplayName` overrides retain precedence, and dynamic row labels require an updated adapter, by @Evangelink in [#11775](https://github.com/microsoft/testfx/pull/11775)
* Add MSTEST0089 to report conflicting constant `DisplayName` and `ArgumentsDisplayName` values in C# and Visual Basic data-row declarations, by @Evangelink in [#11775](https://github.com/microsoft/testfx/pull/11775)

### Changed

* Use direct collection counts and indexers across `Assert.IsNotEmpty`, `Assert.HasCount`, `Assert.IsEmpty`, `Assert.ContainsSingle` and `CollectionAssert` equivalence checks, avoiding unnecessary enumeration for collection-backed inputs, by @Evangelink in [#10980](https://github.com/microsoft/testfx/pull/10980), [#11326](https://github.com/microsoft/testfx/pull/11326) and [#11353](https://github.com/microsoft/testfx/pull/11353)
* Reduce reflection, resource lookup, allocation and array-enumeration overhead when generating data-driven test display names, by @Evangelink in [#11019](https://github.com/microsoft/testfx/pull/11019)
* Escape control characters and unpaired surrogates in automatically generated data-driven test display names while preserving valid surrogate pairs and distinguishing literal escape-sequence text, by @Evangelink in [#11428](https://github.com/microsoft/testfx/pull/11428)
* Avoid double enumeration and unnecessary allocation when converting VSTest test traits into Microsoft.Testing.Platform properties, by @Evangelink in [#11452](https://github.com/microsoft/testfx/pull/11452)
* Reduce one-shot `Assert.That` expression overhead by using interpreted expression compilation where supported, and cache invalid path and file-name character sets used by deployment and test temporary-directory sanitization, by @Evangelink in [#11554](https://github.com/microsoft/testfx/pull/11554), [#11555](https://github.com/microsoft/testfx/pull/11555) and [#11561](https://github.com/microsoft/testfx/pull/11561)
* Run native Microsoft.Testing.Platform MSTest user code inside one canonical test activity so automatic and custom child activities correlate with the logical test without duplicate test spans, by @Evangelink in [#11596](https://github.com/microsoft/testfx/pull/11596)
* Use `UseUwpTools` to select the specialized MSTest UWP application model, allowing `UseUwpTools=false` to retain UWP XAML references while opting into direct Microsoft.Testing.Platform execution, by @Sergio0694 in [#11614](https://github.com/microsoft/testfx/pull/11614)
* Use the separately shipped `Microsoft.Testing.Extensions.PackagedApp.MSBuild` package for `MSTest.Sdk` packaged Windows application execution instead of embedding duplicate controller tools. MSTest metapackage and adapter-only consumers can opt in to the same integration explicitly with `EnableMSTestRunner=true`, by @Evangelink in [#11777](https://github.com/microsoft/testfx/pull/11777)
* Update the Code Coverage extension selected by `MSTest.Sdk` to stable 18.12.0, by @Evangelink in [#11789](https://github.com/microsoft/testfx/pull/11789)
* Synchronize the vendored xxHash implementation with dotnet/runtime, using inline-array state buffers on modern .NET while preserving hash output and stable test IDs, by @Evangelink in [#11771](https://github.com/microsoft/testfx/pull/11771)
* Enable trimming and Native AOT compatibility analysis for modern .NET builds of the MSTest framework and adapter, by @Copilot in [#11729](https://github.com/microsoft/testfx/pull/11729)

### Fixed

* Copy requested classic UWP adapter satellite assemblies independently of the build-machine culture when `EnableMSTestRunner=true`, by @Evangelink in [#11112](https://github.com/microsoft/testfx/pull/11112)
* Prevent the MSTest source generator from crashing on null array attribute arguments such as `[DataRow(null)]`, preserving the value as one null test argument, by @Sergio0694 in [#11371](https://github.com/microsoft/testfx/pull/11371)
* Restore source compatibility when asserting a non-null value against a collection with nullable element types, by @Sergio0694 in [#11374](https://github.com/microsoft/testfx/pull/11374)
* Preserve `TestContext.AddResultFile` attachments for deferred Microsoft.Testing.Platform reporters after MSTest deployment cleanup, including same-named files and long TRX paths, by @Evangelink in [#11606](https://github.com/microsoft/testfx/pull/11606)
* Launch .NET 10 modern UWP tests through Microsoft.Testing.Platform when the AppX recipe names the published executable differently from its source path, by @Evangelink in [#11609](https://github.com/microsoft/testfx/pull/11609)
* Fix MSTest-only and standalone `MSTest.Sdk` restore/build/pack failures caused by missing packaged-app controller restore assets, and keep `MSTest.Extensions.Hosting` prerelease while its Microsoft.Testing.Platform hosting dependency remains alpha, by @Evangelink in [#11773](https://github.com/microsoft/testfx/pull/11773)
* Warn when custom top-level `.runsettings` sections are ignored by MSTest on Microsoft.Testing.Platform, naming the sections and explaining that VSTest `ISettingsProvider` extensions are not invoked, by @Evangelink in [#11785](https://github.com/microsoft/testfx/pull/11785)
* Correct the MSTEST0033 suppression justification to explain that MSTest initializes writable `TestContext` properties rather than requiring constructor initialization, by @Copilot in [#11768](https://github.com/microsoft/testfx/pull/11768)

# Microsoft.Testing.Platform

## <a name="2.5.1" />[2.5.1] - Unreleased

## <a name="2.5.0" />[2.5.0] - 2026-10-05

See full log [of v4.4.1...v4.5.0](https://github.com/microsoft/testfx/compare/v4.4.1...v4.5.0)

### Added

* Introduce an experimental `ITestHostExecutionOrchestratorMiddleware` composed around exactly one existing `ITestHostExecutionOrchestrator` invocation, as a v1 single-invocation composition boundary. Register middleware via the new `AddTestHostExecutionOrchestratorMiddleware` extension method on `ITestHostOrchestratorManager`. This is not a typed multi-run retry/stress/shard engine: middleware cannot repeat, shard, or rewrite the wrapped orchestrator's result, must propagate downstream exceptions and cancellation unchanged, and the existing `--retry-failed-tests` orchestrator is unaffected, by @Evangelink in [#11607](https://github.com/microsoft/testfx/pull/11607)
* Enable native Microsoft.Testing.Platform execution for UWP and AppContainer Windows test hosts through a full-trust sidecar controller, including exact package-SID authorization for controller and extension pipes, LocalState artifact recovery, and multi-host Retry activation, by @Evangelink in [#11484](https://github.com/microsoft/testfx/pull/11484)
* Add provider-independent activation of Microsoft.Testing.Platform activities and metrics, plus focused OpenTelemetry resource helpers for composing test and CI provenance with application-owned providers such as Aspire ServiceDefaults, by @Evangelink in [#11521](https://github.com/microsoft/testfx/pull/11521) and [#11526](https://github.com/microsoft/testfx/pull/11526)
* Add `Microsoft.Testing.Extensions.Configuration` to import a caller-owned Microsoft.Extensions.Configuration snapshot, unambiguous platform-service lookup APIs for applications using Microsoft.Extensions.DependencyInjection, and provider/arity metadata for `dotnet test` command-line options, by @Evangelink in [#11563](https://github.com/microsoft/testfx/pull/11563)
* Generate opt-in C#, Visual Basic and F# entry points that run Microsoft.Testing.Platform inside a caller-owned Microsoft.Extensions.Hosting host, with cooperative cancellation, deterministic host ownership and optional host-owned OpenTelemetry, by @Evangelink in [#11595](https://github.com/microsoft/testfx/pull/11595)
* Add the experimental `ITestingPlatformBuilderConfigurator` extension point so caller-owned host services can configure the Microsoft.Testing.Platform application builder before execution, by @Evangelink in [#11635](https://github.com/microsoft/testfx/pull/11635)
* Add the experimental `ExecuteRequestContext.StartTestExecutionAsync` and `TestExecution` APIs so test frameworks can publish a canonical test lifecycle without depending on internal activity, reservation or message-bus types, by @Evangelink in [#11630](https://github.com/microsoft/testfx/pull/11630)
* Expose experimental `IDiagnosticLoggingInformation` to extensions, providing the active diagnostic log file, minimum level and synchronous-write mode while preserving relocation and disabled-logging semantics, by @Evangelink in [#11653](https://github.com/microsoft/testfx/pull/11653)
* Add `Microsoft.Testing.Extensions.PackagedApp.MSBuild`, an MTP-versioned package containing shared MSBuild targets, a classic UWP bootstrap, and complete .NET 8 and .NET 9 `win-x64` out-of-process controller layouts for packaged Windows test applications. The integration is shared by `MSTest.Sdk` and explicit MSTest metapackage/adapter consumers without adding a Windows controller dependency to ordinary adapter projects, by @Evangelink in [#11777](https://github.com/microsoft/testfx/pull/11777)

### Changed

* Align the CTRF reporter with the first internally consistent upstream specification release by emitting `specVersion: 0.1.0` and pinning documentation to the immutable `v0.1.0` schema, by @Evangelink in [#11731](https://github.com/microsoft/testfx/pull/11731)
* Populate CTRF `testId`, `executionId` and retry `attemptId` fields with stable logical-test and per-execution identities while retaining the bounded `extra.uid` field for compatibility, by @Evangelink in [#11602](https://github.com/microsoft/testfx/pull/11602)
* Correlate asynchronously produced test-result spans with actual framework execution and use canonical MSTest execution activities so automatic and custom child spans retain the correct test topology, by @Evangelink in [#11529](https://github.com/microsoft/testfx/pull/11529) and [#11596](https://github.com/microsoft/testfx/pull/11596)
* Reduce duplicate test-identity computation when merging HTML reports, by @Evangelink in [#11076](https://github.com/microsoft/testfx/pull/11076)
* Replace repeated LINQ passes in Azure DevOps result-date calculations with allocation-free single-pass scans, by @Evangelink in [#11403](https://github.com/microsoft/testfx/pull/11403)
* Enrich and redesign Azure DevOps job summaries with pass rates, flaky-test history, duration comparisons, dependency edges, focused failure diagnostics and compact deterministic multi-module presentation, by @Evangelink in [#11337](https://github.com/microsoft/testfx/pull/11337) and [#11335](https://github.com/microsoft/testfx/pull/11335)
* Reuse a static Jsonite type-dispatch table for .NET Framework and `netstandard2.0` server-mode serialization, avoiding a dictionary and captured delegates on every JSON message, by @Evangelink in [#11522](https://github.com/microsoft/testfx/pull/11522)
* Normalize Retry artifact-recovery containment prefixes once per directory instead of recomputing them for every recovered record and artifact, by @Evangelink in [#11671](https://github.com/microsoft/testfx/pull/11671)
* Use a blocking wait instead of CPU-spinning by default when the VSTest bridge synchronously waits for discovery and result publication, by @Evangelink in [#11786](https://github.com/microsoft/testfx/pull/11786)
* Update the Code Coverage extension dependency to stable 18.12.0, by @Evangelink in [#11789](https://github.com/microsoft/testfx/pull/11789)
* Synchronize TRX reporting's vendored xxHash implementation with dotnet/runtime, using inline-array state buffers on modern .NET while preserving hash output and test-ID generation, by @Evangelink in [#11771](https://github.com/microsoft/testfx/pull/11771)

### Fixed

* Preserve the native `dotnet test` execution ID across PackagedApp AUMID activation so packaged WinUI hosts reconnect to the controller's existing test session instead of generating a conflicting handshake, while retaining the explicit environment allowlist and one-shot cleanup, by @Evangelink in [#11499](https://github.com/microsoft/testfx/pull/11499)
* Re-register the requested PackagedApp development layout when Windows still points the package identity at an older build directory, while preserving application data and refusing to replace conflicting non-development registrations, by @Sergio0694 in [#11372](https://github.com/microsoft/testfx/pull/11372)
* Prevent a cancellation callback ordering race in the Hot Reload extension from allowing another test run after shutdown has started, by @Evangelink in [#11401](https://github.com/microsoft/testfx/pull/11401)
* Keep each Hot Reload iteration active until the test framework reports request completion, avoiding premature message-bus draining and iteration shutdown, by @Copilot in [#11646](https://github.com/microsoft/testfx/pull/11646)
* Log normal cooperative-cancellation pipe closure as a concise lifecycle event instead of an exception-shaped DEBUG diagnostic, by @Evangelink in [#11416](https://github.com/microsoft/testfx/pull/11416)
* Publish Azure DevOps retry metadata and flaky-result fields so tests that pass on rerun display the native flaky badge and retain their attempt history, by @Evangelink in [#11598](https://github.com/microsoft/testfx/pull/11598) and [#11610](https://github.com/microsoft/testfx/pull/11610)
* Launch .NET 10 modern UWP tests when the AppX recipe names the published executable differently from its source path, while preserving exact-path matching and package manifest validation, by @Evangelink in [#11609](https://github.com/microsoft/testfx/pull/11609)
* Reject negative, truncated, undersized and malformed nested `dotnet test` IPC payloads without consuming later fields or allocating from invalid collection counts, by @Evangelink in [#11649](https://github.com/microsoft/testfx/pull/11649)
* Stop publishing additional test results after cancellation and preserve both the original framework exception and any canonical execution-scope cleanup failure, by @Evangelink in [#11666](https://github.com/microsoft/testfx/pull/11666)
* Resolve controller-backed test result paths once at startup so tests that change the current directory cannot redirect or orphan TRX recovery sidecars, while preserving controller ownership of result directories, by @Evangelink in [#11772](https://github.com/microsoft/testfx/pull/11772)
* Serialize shared Azure DevOps run admission, owner election, completion and cleanup so late modules cannot join a closing run or lose their successor's run-id file. Report oversized, missing and unreadable attachment upload failures explicitly while continuing subsequent uploads, by @Evangelink in [#11774](https://github.com/microsoft/testfx/pull/11774)
* Allow in-process server-mode cancellation callbacks to finish within the remaining shutdown grace period before abandoning their cancellation source, by @dotnet-maestro in [#11751](https://github.com/microsoft/testfx/pull/11751)
