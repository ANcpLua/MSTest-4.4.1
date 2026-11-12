# Changelog: MSTest 4.5.1 → 4.5.3

Microsoft.Testing.Platform 2.5.1 → 2.5.3. Source: [microsoft/testfx](https://github.com/microsoft/testfx), tags `v4.5.1` and `v4.5.3`; the changelogs come from `main`.

# MSTest

## <a name="4.5.3" />[4.5.3] - 2026-11-11

See full log [of v4.5.2...v4.5.3](https://github.com/microsoft/testfx/compare/v4.5.2...v4.5.3)

For the Microsoft.Testing.Platform changes included with this release, see [2.5.3](Changelog-Platform.md#2.5.3).

### Changed

* Inspect assembly metadata before loading dependencies during assembly-fixture provider discovery, avoiding unnecessary loads of unmarked libraries while preserving transitive providers, runtime binding and diagnosed fallback, by @Evangelink in [#12291](https://github.com/microsoft/testfx/pull/12291)
* Update the `Microsoft.NET.Test.Sdk` dependency selected by `MSTest.Sdk` to 18.10.2, by @Evangelink in [#12366](https://github.com/microsoft/testfx/pull/12366)

### Fixed

* Allow `UseVSTest=true` for unpackaged WinUI applications in `MSTest.Sdk`, while retaining Microsoft.Testing.Platform as the default host, by @Evangelink in [#12128](https://github.com/microsoft/testfx/pull/12128)
* Exclude copied MSTest adapter satellite assemblies from PRI resource indexing to avoid WinUI packaging build failures, by @dotMorten in [#12145](https://github.com/microsoft/testfx/pull/12145)
* Preserve global test cleanup after a locally canceled or timed-out test cleanup, retain original deadlines and cancellation-source ownership, and aggregate independent cleanup failures without losing timeout or inconclusive outcomes, including nested aggregate messages on .NET Framework, by @Evangelink in [#12220](https://github.com/microsoft/testfx/pull/12220) and [#12292](https://github.com/microsoft/testfx/pull/12292)
* Keep `TestContext.AddResultFile` attachments readable after Microsoft.Testing.Platform host shutdown by retaining copied files in visible `MSTest-<random-id>` result folders, subject to the caller's results-retention policy, by @Evangelink in [#12247](https://github.com/microsoft/testfx/pull/12247)
* Avoid duplicate Microsoft.Testing.Platform runtime DLLs in the classic UWP adapter payload by restoring dependencies from their owning NuGet packages and including the resolved assemblies in AppX deployment, by @Evangelink in [#12248](https://github.com/microsoft/testfx/pull/12248)
* Await all scheduled workers when cancellation occurs during ordinary or dependency-ordered parallel execution, preventing canceled runs from returning before their workers finish, by @Evangelink in [#12297](https://github.com/microsoft/testfx/pull/12297)

## <a name="4.5.2" />[4.5.2] - 2026-10-27

See full log [of v4.5.1...v4.5.2](https://github.com/microsoft/testfx/compare/v4.5.1...v4.5.2)

For the Microsoft.Testing.Platform fixes included with this release, see [2.5.2](Changelog-Platform.md#2.5.2).

### Changed

* Add documentation links to the MSTest source generator's `AOTSG0001` through `AOTSG0005` diagnostics, by @Evangelink in [#11972](https://github.com/microsoft/testfx/pull/11972)
* Update localized MSTest adapter and analyzer messages, by @Evangelink and @nohwnd-bot in [#11898](https://github.com/microsoft/testfx/pull/11898), [#11899](https://github.com/microsoft/testfx/pull/11899), [#11900](https://github.com/microsoft/testfx/pull/11900), [#11973](https://github.com/microsoft/testfx/pull/11973), [#12024](https://github.com/microsoft/testfx/pull/12024) and [#12090](https://github.com/microsoft/testfx/pull/12090)

### Fixed

* Materialize array `Type` arguments such as `[DataRow(typeof(int[]))]` in Native AOT tests, while checking the accessibility of array elements and enclosing generic type arguments, by @Evangelink and @Sergio0694 in [#12030](https://github.com/microsoft/testfx/pull/12030)
* Keep timed UI test initialization, execution and cleanup on their original synchronization context, avoiding WinUI dispatcher deadlocks while preserving timeout and cancellation handling, by @Evangelink in [#12029](https://github.com/microsoft/testfx/pull/12029)
* Allow `MSTEST0032` assertions that intentionally compare a literal with a constant from another assembly to pin a dependency's contract, by @drognanar in [#12087](https://github.com/microsoft/testfx/pull/12087)
* Report `MSTEST0025` for exact-type assertions against abstract classes, and avoid offering an `Assert.Fail` replacement when the assertion's return value is used, by @Evangelink in [#12099](https://github.com/microsoft/testfx/pull/12099)
* Register copied MSTest adapter satellite assemblies in MSBuild's clean manifest so incremental cleanup and `Clean` can remove stale generated localized outputs, by @Copilot in [#12107](https://github.com/microsoft/testfx/pull/12107)

# Microsoft.Testing.Platform

## <a name="2.5.3" />[2.5.3] - 2026-11-11

See full log [of v4.5.2...v4.5.3](https://github.com/microsoft/testfx/compare/v4.5.2...v4.5.3)

### Changed

* Coalesce HangDump activity notifications instead of performing one IPC round trip per test result, while ensuring a dump is never triggered before the configured inactivity timeout, by @thomhurst and @Evangelink in [#12231](https://github.com/microsoft/testfx/pull/12231)
* Distinguish elapsed time from cumulative test time in CI summaries, show bounded Azure DevOps skip-reason groups with platform context, and report authoritative runner-policy exits separately from failed test results, by @Evangelink in [#12149](https://github.com/microsoft/testfx/pull/12149)
* Localize Azure DevOps live-publishing and history diagnostics, by @Evangelink in [#12209](https://github.com/microsoft/testfx/pull/12209)
* Render Azure DevOps skip-reason groups as wrapping list entries rather than tables, and show shared module and platform context once for single-module summaries, by @Evangelink in [#12246](https://github.com/microsoft/testfx/pull/12246)

### Fixed

* Include data-row arguments and custom display names in GitHub Actions failure, flaky-test and slowest-test summaries while retaining fully qualified method names, by @Evangelink in [#12144](https://github.com/microsoft/testfx/pull/12144)
* Preserve Azure DevOps data-row display names and their associated durations while retaining canonical method identities for history. Retry interrupted result-metadata updates without republishing accepted test results, by @Evangelink in [#12163](https://github.com/microsoft/testfx/pull/12163)
* Detect OpenCode through its `OPENCODE` environment variable so AI-agent terminal defaults apply correctly, by @Evangelink in [#12178](https://github.com/microsoft/testfx/pull/12178)
* Diagnose uncorrelated JSON-RPC errors with `id: null` without ending server sessions, replying to the error or completing unrelated pending client requests, while preserving request accounting, by @Evangelink in [#12286](https://github.com/microsoft/testfx/pull/12286)
* Correct TRX artifact publication with Retry, moving the final default or relative report to the results directory while retaining earlier attempt reports under `Retries`, preserving attachment paths and forwarding relocated artifacts through native `dotnet test` and packaged-app controllers, by @Evangelink in [#12290](https://github.com/microsoft/testfx/pull/12290)
* Preserve nested and aggregate exception causes in CTRF, HTML and JUnit reports while retaining explicit failure explanations and existing simple-exception stack traces, by @Evangelink in [#12297](https://github.com/microsoft/testfx/pull/12297)
* Preserve complete CTRF attachment paths instead of truncating references longer than 4,096 characters, while retaining diagnostic size limits for attachment names and descriptions, by @Evangelink in [#12359](https://github.com/microsoft/testfx/pull/12359)

## <a name="2.5.2" />[2.5.2] - 2026-10-27

See full log [of v4.5.1...v4.5.2](https://github.com/microsoft/testfx/compare/v4.5.1...v4.5.2)

### Changed

* Reduce startup work when parsing build metadata, validating extension options and resolving services, by @thomhurst in [#12085](https://github.com/microsoft/testfx/pull/12085) and [#12086](https://github.com/microsoft/testfx/pull/12086)
* Increase the Azure DevOps live-publishing attachment limit from 16 MiB to 75 MiB per file, while continuing to report oversized or unreadable attachments without failing otherwise successful tests, by @Evangelink in [#12101](https://github.com/microsoft/testfx/pull/12101)
* Update the MessagePack dependency to 3.1.11, by @Evangelink in [#12026](https://github.com/microsoft/testfx/pull/12026)
* Update localized platform, Azure DevOps report, GitHub Actions report and packaged-app messages, by @Evangelink and @nohwnd-bot in [#11898](https://github.com/microsoft/testfx/pull/11898), [#11899](https://github.com/microsoft/testfx/pull/11899), [#11900](https://github.com/microsoft/testfx/pull/11900), [#11973](https://github.com/microsoft/testfx/pull/11973), [#12025](https://github.com/microsoft/testfx/pull/12025) and [#12090](https://github.com/microsoft/testfx/pull/12090)
* Mark blocking file-logger disposal as unsupported on browser and WASI runtimes, where shutdown must await asynchronous disposal, by @Evangelink in [#12098](https://github.com/microsoft/testfx/pull/12098)

### Fixed

* Restore native `dotnet test` routing for packaged full-trust Windows applications, including project-local controller staging, preserved launch-profile arguments and informational help/discovery requests, by @Sergio0694 and @Evangelink in [#11818](https://github.com/microsoft/testfx/pull/11818) and [#11854](https://github.com/microsoft/testfx/pull/11854)
* Forward MSTest's `--filter` expression through the packaged-app sidecar for native `dotnet test` and `InvokeTestingPlatform`, preserving argument boundaries and leaving filter evaluation to the activated host, by @Sergio0694 in [#11837](https://github.com/microsoft/testfx/pull/11837)
* Generate sidecar launcher configuration for packaged Test Explorer sources without overwriting caller-owned launcher files, and accept Test Explorer's JSON-RPC server arguments for discovery and execution, by @Evangelink in [#12054](https://github.com/microsoft/testfx/pull/12054)
* Preserve a shared logical CTRF `runId` across coordinated modules and retries without substituting a module-local `dotnet test` execution ID, by @Evangelink in [#11832](https://github.com/microsoft/testfx/pull/11832)
* Keep CTRF execution lifecycles and physical retry-attempt identities distinct, preserving stable lifecycle IDs through retry consolidation and retaining ambiguous or legacy results without guessing their history, by @Evangelink in [#11835](https://github.com/microsoft/testfx/pull/11835)
* Preserve the invocation working directory for ordinary controller-launched test hosts so relative paths such as `--coverage-settings test.runsettings` keep working under `dotnet run`, while retaining the configured working directory for custom test host launchers, by @Evangelink in [#11858](https://github.com/microsoft/testfx/pull/11858)
* Escape non-ASCII discovery JSON characters so headless Windows console encodings cannot corrupt test names or introduce invalid JSON control characters, by @Evangelink in [#11904](https://github.com/microsoft/testfx/pull/11904)
* Actually remove environment variables requested by a test-host configuration provider instead of retaining their previous values, by @Evangelink in [#11974](https://github.com/microsoft/testfx/pull/11974)
* Align terminal summary verdicts with framework-session failures, minimum executed-test counts that exclude skipped tests, and explicit zero-test allowances, by @Evangelink in [#12031](https://github.com/microsoft/testfx/pull/12031)
* Write warnings and errors to the console when startup fails before the native `dotnet test` pipe handshake, allowing the SDK to surface the failure diagnostics, by @Evangelink in [#12027](https://github.com/microsoft/testfx/pull/12027)
* Preserve GitHub Actions groups, failure/skip annotations and slow-test notices through native `dotnet test` when the SDK negotiates protocol 1.3.0 or later; older SDKs require running the test executable directly for those workflow commands, by @Evangelink in [#12055](https://github.com/microsoft/testfx/pull/12055)
* Restore MSBuild module information and result reporting for ordinary controller-backed test hosts while keeping AppContainer children off the outer build pipe, and create the result directory before initializing the module log, by @Evangelink in [#12057](https://github.com/microsoft/testfx/pull/12057)
* Correct the experimental diagnostic documentation link for `IChatClientProvider`, by @Evangelink in [#11901](https://github.com/microsoft/testfx/pull/11901)
