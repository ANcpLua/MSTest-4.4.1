# Retained upstream MSBuild/TRX fix

[mstest-2.5.1-msbuild-trx.patch](mstest-2.5.1-msbuild-trx.patch) targets
`microsoft/testfx` tag `v4.5.1`, commit `0bf77e3aedbf86f1fa16220bc951d3b52ba56763`.
The [source manifest](mstest-2.5.1-msbuild-trx.sources.json) records the resulting file hashes.
Patch SHA-256: `de80963077f054d1f47d2383e393c8e4ae2d4c9667b60efed275274c6e6cfa66`.

Published MTP 2.5.1 loses MSBuild completion messages when TRX starts a controller-managed
host. The patch relays the existing messages through an authorized controller endpoint;
it does not infer success from process exit or weaken failed/skipped outcomes. Ordinary
runs do not acquire an unnecessary controller. PackagedApp forwards the relay metadata
through its existing allowlist without broadening pipe authorization.

Local verification on macOS with SDK 10.0.401 and MSTest 4.5.1:

| Run | Observed result |
|---|---|
| Published baseline, TRX enabled | Three tests pass; MSBuild exits 1 with missing module information |
| Candidate, TRX enabled | Three tests pass; MSBuild exits 0; exact inherited JSON sentinel and TRX verified |
| Candidate, plain MSBuild | Three tests pass; MSBuild exits 0 |
| Candidate, failed/skipped fixture | MSBuild exits 1; exact failure diagnostics and matching TRX outcomes |
| Controller observer regression tests | Four passed |
| PackagedApp allowlist regression | One passed |

Source builds succeeded for net8.0, net9.0 and netstandard2.0. Those upstream builds retain
171 warnings in unchanged code; they are not a zero-warning gate. The full upstream suite,
Windows AppContainer activation/ACL behavior, signing and release gates remain unverified.

This showcase continues to use official packages. No patched DLL was installed in it or
in the NuGet cache. Delivering this fix requires coordinated `Microsoft.Testing.Platform`
and `Microsoft.Testing.Platform.MSBuild` packages; Windows packaged consumers also need
matching PackagedApp and PackagedApp.MSBuild payloads. The strict official-package
reproduction remains [verify-msbuild-trx.sh](../scripts/verify-msbuild-trx.sh).
