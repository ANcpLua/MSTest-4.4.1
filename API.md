# MSTest 4.5.1 examples

The examples compile against MSTest 4.5.1 and Microsoft.Testing.Platform 2.5.1.
This is a navigation index, not a claim that every public API has an example.
The [scout delta](delta/4.5.1/api.md) lists added APIs; the matching
[microsoft/testfx source](https://github.com/microsoft/testfx/tree/v4.5.1) defines their contracts.

| Topic | Runnable source | Contract exercised |
|---|---|---|
| Collection assertions | [Assertions](MSTest.Assertions%204.5.1/Spans) | Span/memory acceptance and exact assertion failures, custom comparers, structural equality |
| C# 12 overloads | [SourceCompatibility](MSTest.Assertions.CSharp12%204.5.1/SourceCompatibility) | Arrays, strings, array segments and custom conversions |
| Failure text | [Differences](MSTest.Assertions%204.5.1/Differences) | Whitespace markers and bounded comparison previews |
| Data providers | [Combinatorial](MSTest.Data%204.5.1/Combinatorial) | Cartesian order, explicit/inferred values, random-provider caching and validation |
| Row names | [ArgumentLabelTests](MSTest.Data%204.5.1/DisplayNames/ArgumentLabelTests.cs) | ArgumentsDisplayName on inline and dynamic rows, full-name precedence |
| Conditions | [Conditions](MSTest.Conditions%204.5.1) | Architecture, CI, executable and class conditions |
| Analyzer diagnostics | [Diagnostics](MSTest.Analyzers%204.5.1/Diagnostics) | Actual Roslyn diagnostics for declarations, row names, constant assertions, unsafe global state and early-return guards |
| Fixture context | [Context](MSTest.Execution%204.5.1/Context) | Per-test and class/assembly context availability |
| Timeouts and deadline | [FixtureRunTests](MSTest.Execution%204.5.1/Timeouts/FixtureRunTests.cs), [DeadlineRunTests](MSTest.Execution%204.5.1/Deadline/DeadlineRunTests.cs) | Native child results and TRX for fixture timeouts, cancellation and deadline termination |
| Live output | [LiveOutputTests](MSTest.Execution%204.5.1/Output/LiveOutputTests.cs) | Output arrives before completion; cancellation owns the child process |
| Host dependency injection | [Hosting.Experimental](MSTest.Hosting.Experimental%204.5.1) | Host configuration, scoped services, independent data rows and lifecycle state; prerelease packages |
| Resource locks | [Isolation](MSTest.Isolation%204.5.1/ResourceLocks), [ClassLevel](MSTest.Isolation.ClassLevel%204.5.1/ResourceLocks) | Exclusive/shared locks, keys, inherited locks, class-level ownership |
| Test files | [TempDirectory](MSTest.Isolation%204.5.1/TempDirectory) | Per-row directories, access mode, cleanup and retention |
| Dependencies | [OrderingRunTests](MSTest.Ordering%204.5.1/OrderingRunTests.cs) | 65 native scenarios, prerequisite failures and configured dependency chains |
| Dependency cycles | [CycleRunTests](MSTest.Ordering.Cycles%204.5.1/CycleRunTests.cs) | Failed cyclic nodes and untouched bodies |
| Run filtering | [Filtering](MSTest.Filtering%204.5.1/Registration) | Run, skip and drop decisions; generic provider constraints |
| Source generation | [SourceGeneration](MSTest.SourceGeneration%204.5.1) | Generated invocation, descriptors, fallback reflection, metadata and literal preservation |
| Native AOT | [NativeAot](MSTest.NativeAot%204.5.1) | Generated invocation and trimming; native publication is a separate verification step |
| Assertion artifacts | [DiagnosticArtifactRunTests](MSTest.Reporting%204.5.1/Assertions/DiagnosticArtifactRunTests.cs) | Values, source frames and test identity from opt-in assertion diagnostics |
| Test telemetry | [Reporting spans](MSTest.Reporting%204.5.1/Spans), [Platform telemetry](MSTest.Platform.Changed%204.5.1/Telemetry) | Canonical test/fixture spans, errors, instrument names, resource and trace relationships |
| Retry results | [Retry](MSTest.Retry%204.5.1) | Attempt history, final outcomes and reporter output from real child runs |
| MTP services and reports | [Platform](MSTest.Platform%204.5.1) | Builder services, filters, process control, reports and extension loading |
| MTP behavior | [Changed](MSTest.Platform.Changed%204.5.1), [Fixed](MSTest.Platform.Fixed%204.5.1) | Executable examples with asserted process/report outcomes |
| Browser WASM | [Browser solution](MSTest.Browser.slnx) | Published JavaScript/Node host; distinct from desktop execution |

Windows UI automation and packaged Windows application activation cannot be executed on the
macOS verification host. The matching framework documentation covers those APIs; this repository
does not label a source-only example as a verified Windows run.

The per-method review is in [review/test-review.json](review/test-review.json). It separates
source review from execution evidence. Run evidence under `artifacts/mstest-wave-20261016`
records actual cases, including intentionally failing child fixtures; it is ignored by Git.

MTP 2.5.1 has a separately reproduced MSBuild/TRX defect: adding `--report-trx` to
`dotnet build -t:Test` makes the three environment examples pass, but the build returns 1
because the controller does not relay the MSBuild completion messages. Run
`scripts/verify-msbuild-trx.sh` to reproduce it. The environment-inheritance example checks
native detailed output and its own structured sentinel record; it does not claim this TRX
integration works. A verified local source patch and its delivery limits are retained in [patches](patches/README.md); official packages remain affected.
