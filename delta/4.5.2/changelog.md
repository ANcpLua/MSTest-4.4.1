# Changelog: MSTest 4.5.1 → 4.5.2

Microsoft.Testing.Platform 2.5.1 → 2.5.2. Source: [microsoft/testfx](https://github.com/microsoft/testfx), tags `v4.5.1` and `v4.5.2`; the changelogs come from `main`.

# MSTest

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
