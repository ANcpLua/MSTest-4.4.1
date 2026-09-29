# Build the MSTest 4.4.1 showcase

You build the repository `MSTest-4.4.1`: runnable examples of the complete surface that MSTest 4.4.1 on
Microsoft.Testing.Platform 2.4.1 offers a test author. People copy from it and cite it. It is held to one
standard: **the cleanest current usage, and no statement that was not read in a source or observed in a run.**

This document is the brief and the decision record. Every decision carries its reason, so that you can
judge the cases it does not name.

## Pins

| | |
|---|---|
| .NET SDK | 10.0.401, `rollForward: latestFeature` |
| Target framework | `net10.0` |
| Language | C# 14 |
| MSTest.Sdk | 4.4.1, pinned in `global.json` under `msbuild-sdks` |
| Microsoft.Testing.Platform | 2.4.1 |
| Machine | macOS arm64 |

## The present tense

The repository describes what is. This one idea decides most questions of scope and wording.

- **One platform.** Microsoft.Testing.Platform runs every test. Projects use `MSTest.Sdk`, start with
  `dotnet run` and `dotnet test`, and are configured by `testconfig.json`, MSBuild properties,
  environment variables and command-line options.
- **One time.** A version number stands in the repository name, the solution name, the project folder
  names, the pin files and the first line of `README.md` and `API.md`. Code, comments and catalog rows
  speak about the API as it is.
- **Stable is the default.** Whatever the pinned packages ship without `[Experimental]` is simply the API.
- **Experimental is shown, and marked.** An API that carries `[Experimental]` in the pinned packages is
  part of the showcase. It lives in a project whose name ends in `.Experimental`, the project file
  acknowledges the diagnostic id (`MSTESTEXP`, `TPEXP`), and its catalog rows say `experimental`.
  Reason: a reader sees at the project boundary which examples may change, and stable projects stay free
  of suppressions.

Guardrail, checked at the end: this command prints nothing.

```sh
grep -rniE 'vstest|runsettings|Microsoft\.NET\.Test\.Sdk|new in |introduced in|since (version |MSTest |[0-9])|previously|no longer|used to |graduat|legacy|migrat' \
  --include='*.cs' --include='*.csproj' --include='*.props' --include='*.json' --include='*.md' \
  --exclude='PROMPT.md' --exclude-dir=artifacts --exclude-dir=delta .
```

## Sources, in order of authority

Fetch them into one folder outside the repository, here called `ref/`.

| Source | Fetch | What it is |
|---|---|---|
| `ref/testfx` | `git clone --depth 1 --branch v4.4.1 https://github.com/microsoft/testfx.git` | The pinned source: `src/`, `samples/public/`, `test/`. The acceptance and integration tests show intended usage and expected output. |
| `ref/mstest-docs` | `gh repo clone ANcpLua/mstest-docs` | Guides and API reference for exactly these pins. Read its `CLAUDE.md` first. `docs/api` is generated from the 4.4.1 and 2.4.1 assemblies and is the proof of what exists. |
| Changelogs | `docs/Changelog.md` and `docs/Changelog-Platform.md` of microsoft/testfx on `main` | Entries are still added to a release's section after its tag, so `main` is the complete text. Sections 4.5.0 and 2.5.0 are unreleased and describe nothing in the pinned packages. |
| `ref/delta` | `python3 scripts/scout.py --from 4.3.3 --to 4.4.1 --output ref/delta` | 531 public API lines and 18 analyzer rows that 4.4.1 has and 4.3.3 lacks. A checklist for completeness, never a source of wording. |
| `ref/TUnit-1.68.17` | `gh repo clone ANcpLua/TUnit-1.68.17` | The owner's TUnit showcase. The model for layout, `README.md`, `API.md` and `CLAUDE.md`. |

**Evidence rule.** State no API name, signature, default, option, output line or behaviour from memory.
Read it in a source above or observe it in a run, and keep the path with line, or the output line, next to
the statement until the catalog row is written.

## Scope: the whole surface

Every item below ends in a catalog row with a runnable example, or in the list `Not shown` with a reason
that holds on this machine.

### MSTest.TestFramework 4.4.1: 87 public types

| Topic | Types |
|---|---|
| Tests | `TestClassAttribute`, `TestMethodAttribute`, `STATestClassAttribute`, `STATestMethodAttribute`, `DiscoverInternalsAttribute`, `ITestMethod`, `TestResult`, `UnitTestOutcome` |
| Lifecycle | `AssemblyInitializeAttribute`, `AssemblyCleanupAttribute`, `ClassInitializeAttribute`, `ClassCleanupAttribute`, `TestInitializeAttribute`, `TestCleanupAttribute`, `GlobalTestInitializeAttribute`, `GlobalTestCleanupAttribute`, `AssemblyFixtureProviderAttribute`, `InheritanceBehavior`, `TestContext` |
| Data | `DataRowAttribute`, `DynamicDataAttribute`, `DynamicDataSourceType`, `TestDataRow<T>`, `ITestDataSource`, `ITestDataSourceIgnoreCapability`, `ITestDataSourceEmptyDataSourceExceptionInfo`, `TestDataSourceOptionsAttribute`, `TestDataSourceUnfoldingStrategy`, `DataSourceAttribute`, `DataAccessMethod`, `CombinatorialDataAttribute`, `CombinatorialValuesAttribute`, `CombinatorialRangeAttribute`, `CombinatorialRandomDataAttribute`, `ICombinatorialValuesProvider` |
| Assertions | `Assert`, `AssertExtensions`, `StringAssert`, `CollectionAssert`, `AssertFailedException`, `AssertInconclusiveException`, `UnitTestAssertException` |
| Conditions | `ConditionBaseAttribute`, `ConditionMode`, `OSConditionAttribute`, `OperatingSystems`, `ArchitectureConditionAttribute`, `TestArchitectures`, `CIConditionAttribute`, `ExecutableConditionAttribute`, `MemberConditionAttribute`, `IgnoreAttribute` |
| Execution | `ParallelizeAttribute`, `DoNotParallelizeAttribute`, `ExecutionScope`, `ResourceLockAttribute`, `ResourceAccessMode`, `WellKnownResources`, `DependsOnAttribute`, `SequenceOrder`, `TimeoutAttribute`, `RetryAttribute`, `RetryBaseAttribute`, `RetryContext`, `RetryResult`, `DelayBackoffType` |
| Metadata | `TestCategoryAttribute`, `TestCategoryBaseAttribute`, `OwnerAttribute`, `PriorityAttribute`, `DescriptionAttribute`, `TestPropertyAttribute`, `WorkItemAttribute`, `GitHubWorkItemAttribute` |
| Filtering and run information | `ITestFilter`, `TestFilterProviderAttribute`, `TestFilterProviderAttribute<T>`, `TestFilterContext`, `TestFilterResult`, `TestFilterAction`, `TestRun`, `ITestRunInfo`, `PlannedTest` |
| Output and files | `Logger`, `Logger.LogMessageHandler`, `MessageLevel`, `DeploymentItemAttribute` |

Every public member of these types is covered, overloads grouped where they teach the same thing.

**`Assert` has 48 method names**, each with a passing use and the failure message it produces:
`AddValueFormatter`, `AreAllDistinct`, `AreAllNotNull`, `AreAllOfType`, `AreEqual`, `AreEquivalent`,
`AreNotEqual`, `AreNotEquivalent`, `AreNotSame`, `AreNotSequenceEqual`, `AreSame`, `AreSequenceEqual`,
`Contains`, `ContainsAll`, `ContainsSingle`, `DoesNotContain`, `DoesNotContainAll`, `DoesNotEndWith`,
`DoesNotMatchRegex`, `DoesNotStartWith`, `EndsWith`, `Fail`, `HasCount`, `Inconclusive`, `IsEmpty`,
`IsExactInstanceOfType`, `IsFalse`, `IsGreaterThan`, `IsGreaterThanOrEqualTo`, `IsInRange`,
`IsInstanceOfType`, `IsLessThan`, `IsLessThanOrEqualTo`, `IsNegative`, `IsNotEmpty`,
`IsNotExactInstanceOfType`, `IsNotInstanceOfType`, `IsNotNull`, `IsNull`, `IsPositive`, `IsTrue`,
`MatchesRegex`, `Scope`, `StartsWith`, `Throws`, `ThrowsAsync`, `ThrowsExactly`, `ThrowsExactlyAsync`,
the property `Assert.That` (the instance that custom assertions extend) and `AssertExtensions.That`
(the assertion over a Boolean expression). Include the overloads for spans and memory, for comparers, for
interpolated messages, and the members of `StringAssert` and `CollectionAssert`.

**`TestContext` has 20 members**: `CancellationToken`, `Current`, `CurrentTestOutcome`, `DeploymentDirectory`,
`FullyQualifiedTestClassName`, `Properties`, `ResultsDirectory`, `TestData`, `TestDisplayName`,
`TestException`, `TestName`, `TestResultsDirectory`, `TestRunCount`, `TestRunDirectory`,
`TestRunResultsDirectory`, `TestTempDirectory`, `AddResultFile`, `DisplayMessage`, `Write`, `WriteLine`.

**Experimental in the pinned packages** (`MSTESTEXP`): `Assert.AddValueFormatter`, `Assert.Scope`,
`TestContext.Current`, `ITestFilter`, `TestFilterProviderAttribute`, `TestFilterProviderAttribute<T>`,
`TestFilterContext`, `TestFilterResult`, `TestFilterAction`, `TestRun`, `ITestRunInfo`, `PlannedTest`,
`RetryBaseAttribute`, `RetryContext`, `RetryResult`, `ITestDataSourceEmptyDataSourceExceptionInfo`.
Confirm the list against `docs/api` before you rely on it; `[Experimental]` can sit on single members.

### Analyzers

All 83 documented rules under `ref/mstest-docs/docs/mstest/analyzers/`, plus `AOTSG0001` to `AOTSG0005`
of the source generator. Each rule gets the flagged pattern, the accepted form, the diagnostic line as the
compiler prints it, and the result of its code fix where it has one.

### MSTest.Sdk and configuration

- Project properties: `TestingExtensionsProfile` (`Default`, `AllMicrosoft`, `None`), every
  `EnableMicrosoftTestingExtensions…` switch, `EnableMSTestSourceGeneration`, `EnableAspireTesting`,
  `EnablePlaywright`, `IsTestApplication`, `GenerateTestingPlatformEntryPoint`,
  `TestingPlatformCommandLineArguments`, the `TestingPlatformEnvironmentVariable` item, `MSTestAnalysisMode`.
- `testconfig.json`: every key of the sections `assemblyResolution`, `deployment`, `output`, `parallelism`,
  `execution` (with `dependencies`: `chains` and `nodes`) and `timeout`.
- Environment variables that MSTest and the platform read.
- Source generation and Native AOT: `EnableMSTestSourceGeneration` on the normal runtime, a project
  published with `PublishAot`, and how a reader tells that the generated path ran.

### Microsoft.Testing.Platform 2.4.1

- Every command-line option of the platform and of each extension, with the output it produces.
- Every extension Microsoft ships: TRX, HTML, JUnit and CTRF reports, GitHub Actions and Azure DevOps
  reports, code coverage with summary and thresholds, crash dump, hang dump, retry of failed tests,
  hot reload, telemetry, OpenTelemetry, artifact post-processing and report merging under `dotnet test`.
- Exit codes and their meaning.
- Server mode and its client.
- WebAssembly: a `browser-wasm` test application builds and runs on this machine without a workload.

## Extensions: the centrepiece

A framework is judged by how it bends. Each extension point gets its own example: a small, real
extension with a name from a domain, a test that proves it took effect, and a test that proves what
happens without it.

### Extending MSTest

| Extension point | Build |
|---|---|
| `TestMethodAttribute` (virtual execution) | an attribute that wraps the test run, for example one that runs the test under a fixed culture and restores it |
| `TestClassAttribute` (virtual) | a test class attribute that supplies its own test method attribute |
| `DataRowAttribute` (unsealed) | a typed row attribute for one domain value |
| `ITestDataSource` with `ITestDataSourceIgnoreCapability` and `TestDataRow<T>` | a data attribute that generates rows, names them, ignores single rows and attaches categories |
| `ICombinatorialValuesProvider` | a parameter attribute that supplies the values of one parameter |
| `ConditionBaseAttribute` | a condition with its own `GroupName`, combined with a built-in condition in the same and in another group |
| `TestCategoryBaseAttribute` | a category attribute that carries several categories |
| `UnitTestAssertException` | an assertion exception of its own, and how the run reports it |
| `Assert.That` with C# 14 extension members | custom assertions as extension members of `Assert`: the instance form called as `Assert.That.IsPrime(value)`, the static form called as `Assert.IsPrime(value)`, both throwing `AssertFailedException` and with `[CallerArgumentExpression]` in the message |
| `Logger.OnLogMessage` | a listener for what `Logger.LogMessage` writes |
| `RetryBaseAttribute` (experimental) | a retry policy that decides by exception type and exposes every attempt through `RetryResult.AllResults` |
| `ITestFilter` with `TestFilterProviderAttribute<T>` (experimental) | a filter that runs, drops and skips by metadata; one provider per assembly |
| `AssemblyFixtureProviderAttribute`, `TestRun.Current` (experimental) | a shared fixture that starts only when a planned test needs it |
| `Assert.Scope`, `Assert.AddValueFormatter` (experimental) | several failures reported together; a formatter for a domain type |

### Extending the platform

A custom entry point (`GenerateTestingPlatformEntryPoint` set to `false`) registers each of these, and a
test or a recorded run proves that it was called:

| Namespace | Interfaces |
|---|---|
| `Microsoft.Testing.Platform.Extensions` | `IExtension`, `IAsyncInitializableExtension`, `IAsyncCleanableExtension`, `IDataConsumer`, `IBlockingDataConsumer` |
| `…Extensions.TestHost` | `ITestHostExtension`, `ITestSessionLifetimeHandler`, `ITestHostApplicationLifetime` |
| `…Extensions.TestHostControllers` | `ITestHostControllersExtension`, `ITestHostEnvironmentVariableProvider`, `ITestHostProcessLifetimeHandler`, `ITestHostLauncher`, `ITestHostControllerConnectionAuthorizer` |
| `…Extensions.CommandLine` | `ICommandLineOptionsProvider` |
| `…Extensions.OutputDevice` | `IOutputDeviceDataProducer` |
| `…Extensions.TestHostOrchestrator` | `ITestHostOrchestratorExtension`, `ITestHostExecutionOrchestrator`, `ITestHostOrchestratorApplicationLifetime` |
| `…Extensions.ArtifactPostProcessing` | `IArtifactPostProcessor`, `IArtifactPostProcessingManager`, `IArtifactPostProcessorRequiresPostProcessing` |

Also: the platform services an extension can request, the capabilities, configuration through
`IConfiguration`, and the message types a data consumer receives.

## Decisions for the code

An example teaches MSTest. The C# around it is the best current C#, and it stays out of the way.

### Language

Use these where they make the example shorter or clearer; an example that needs a comment to explain its
C# uses too much of it.

| Feature | Where it belongs here |
|---|---|
| File-scoped namespaces, implicit usings, nullable enabled | everywhere |
| Primary constructors | test classes that receive `TestContext`; fixtures; small domain types |
| Collection expressions and spread | expected values, data rows, arguments |
| Raw string literals | expected output, JSON, multi-line messages |
| Records and `required` members | domain values in examples, data rows of `TestDataRow<T>` |
| Pattern matching, list patterns, switch expressions | decisions inside extensions |
| `field` keyword | properties with a guard |
| Extension members (C# 14 extension blocks) | custom assertions on `Assert`, helpers on `TestContext` |
| Null-conditional assignment | optional wiring in fixtures |
| `nameof` of unbound generic types | names of generic fixtures and attributes |
| `params` collections and span overloads | assertion helpers, the span and memory assertions |
| `System.Threading.Lock` | shared state in parallel examples |
| `TimeProvider` | every example in which time matters |
| `await using`, `IAsyncDisposable` | asynchronous cleanup |
| Static abstract interface members, generic attributes | typed data sources and providers |

### Constants for strings

A string that names something is a constant: a category, a resource lock, a property key, an environment
variable, a display name, a work item, a configuration key, a message that a test compares. It is a
`private const` in the class that owns it, or a member of one `internal static class` per project when
several classes share it. Members are referenced with `nameof`.
Reason: the name is written once, a rename is one edit, and a typo becomes a compile error. For resource
locks the analyzer asks for the same (MSTEST0073).

### Private helpers

- What two tests of a class share goes into a `private static` method or a local function of that class,
  named from the domain for what it decides or builds: `PlaceOrder`, `ShippedOrder`.
- What several classes share goes into `Support/` of the project as an `internal static class`.
- The API under demonstration stays in the test body. A helper may build the order; it never hides the
  attribute, the assertion or the option the example is about.
  Reason: the reader copies the test body. What is not visible there is not taught.

### Setups

| Need | Form |
|---|---|
| Per-test state that is built synchronously | the constructor and `readonly` fields; `TestContext` as a constructor parameter |
| Per-test setup that awaits | `[TestInitialize]` returning `Task` |
| Per-test cleanup | `Dispose` or `DisposeAsync`; `[TestCleanup]` when cleanup needs the outcome of the test |
| State shared by a class | `[ClassInitialize]` and `[ClassCleanup]`, with `InheritanceBehavior` shown on a base class |
| State shared by the assembly | `[AssemblyInitialize]` and `[AssemblyCleanup]` |
| Code around every test of the assembly | `[GlobalTestInitialize]` and `[GlobalTestCleanup]` |
| Files | `TestContext.TestTempDirectory` |
| Cancellation | `TestContext.CancellationToken` passed to every awaitable that takes one |

Show the complete order of the lifecycle in one example that records it and asserts the recorded order.
Confirm each row against `ref/mstest-docs/docs/mstest/writing-tests/lifecycle.md` and `testcontext.md`.

### Tests

- Every class is `sealed` and has one XML summary that says what the example teaches.
- Test names read as sentences: `Ships_after_the_order_was_paid`.
- The most specific assertion is used: `Assert.HasCount`, never a comparison inside `Assert.IsTrue`.
- Each example asserts the behaviour it teaches. **Sensitivity:** the test fails when the feature is
  removed. Prove it once on a scratch copy outside the repository.
- Tests are deterministic: a `TimeProvider` instead of the clock, a wait handle instead of a delay, no
  network, no path outside `TestTempDirectory`.
- A demonstration that fails, times out or is cancelled on purpose carries `[Explicit]` from
  `Shared/ExplicitAttribute.cs` (skipped unless `MSTEST_EXPLICIT=1`), and its summary states the outcome.
  The ordinary run of every project ends with exit code 0.
- An analyzer's flagged pattern stands between `#pragma warning disable` and `#pragma warning restore`,
  next to the accepted form.

### Projects

- `Directory.Build.props` holds what all projects share: `net10.0`, nullable, implicit usings, artifacts
  output, `TreatWarningsAsErrors`, `MSTestAnalysisMode` set to `Recommended`. Package versions are managed
  centrally.
- Every assembly states its parallelisation in `AssemblyPolicies.cs` (MSTEST0001 is an error here).
- A project is `<Name> 4.4.1/<Name> 4.4.1.csproj`.

## Layout

Projects follow topics. Adapt the cut when a topic turns out too large or too small.

| Project | Shows |
|---|---|
| `MSTest 4.4.1` | the smallest complete project: where a reader starts |
| `MSTest.Assertions 4.4.1` | `Assert`, `StringAssert`, `CollectionAssert`, `Assert.That`, failure messages |
| `MSTest.Data 4.4.1` | data rows, dynamic data, typed rows, combinatorial data, display names, unfolding |
| `MSTest.Lifecycle 4.4.1` | setups, fixtures, `TestContext`, timeouts, cancellation |
| `MSTest.Execution 4.4.1` | parallelism, resource locks, dependencies and order, retry, conditions, output |
| `MSTest.Metadata 4.4.1` | categories, owner, priority, properties, work items, filtering on the command line |
| `MSTest.Extensions 4.4.1` | every stable extension point of the framework |
| `MSTest.Extensions.Experimental 4.4.1` | every experimental extension point; further `.Experimental` projects where one assembly allows one provider |
| `MSTest.Analyzers 4.4.1` | every rule |
| `MSTest.SourceGeneration 4.4.1`, `MSTest.NativeAot 4.4.1` | generated discovery, Native AOT |
| `MSTest.Configuration 4.4.1` | `testconfig.json`, MSBuild properties, environment variables |
| `MSTest.Platform 4.4.1` | command-line options, exit codes, terminal output |
| `MSTest.Platform.Reports 4.4.1` | reports, coverage, dumps, retry of failed tests, telemetry |
| `MSTest.Platform.Extensions 4.4.1` | writing platform extensions, custom entry point, services, server mode |
| `MSTest.Aspire 4.4.1`, `MSTest.Playwright 4.4.1` | `EnableAspireTesting`, `EnablePlaywright` |
| `MSTest.WebAssembly 4.4.1` | a test application on `browser-wasm` |

Root files: `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.gitignore`,
`MSTest 4.4.1.slnx`, `README.md`, `API.md`, `CLAUDE.md`, `Shared/ExplicitAttribute.cs`,
`scripts/scout.py`, `.github/workflows/scout.yml`. The last three exist and stay.

## Documents

- **`API.md`**: one table per topic. Columns: API, Namespace, Kind, Status (`stable` or `experimental`),
  Example, Verified by, Observed, Note. A last section `Not shown` lists what has no example, with the
  reason and the source line that proves the reason. Generate the file from one data file per project,
  so that no row is typed twice.
- **`README.md`**: what the repository is, the project table, the commands, and the counts of the last
  full run with date, SDK and machine. Only counts that the final run printed.
- **`CLAUDE.md`**: the conventions of this document in the short form a maintainer needs, and the steps
  of a version update: fetch the scouting pull request, read `delta/<version>/`, add examples for what
  is new, rename the versioned paths, run everything, update the counts.

## Method

Work topic by topic, and finish a topic before the next one starts.

1. **Read.** For each API: the implementing source, one upstream test or sample, the guide page.
2. **Write** the example and its support code.
3. **Run.** Build with 0 warnings. Run the project. Run every `[Explicit]` demonstration.
4. **Break.** Remove the feature on a scratch copy; the test has to fail.
5. **Record** the catalog row from what the run printed, and write the project's data file to disk.
6. **Refute.** A second reader who did not write the example gets the rows and the code, rebuilds,
   reruns, and tries to prove each statement false. What it proves is corrected; then the counts of all
   rows of the project are brought to the final run.

An interruption then costs one step, because every finished step is on disk.

### Traps that the first attempt fell into

An earlier attempt at this repository was reviewed adversarially. Every area had false statements in its
first draft, of these kinds:

| Trap | Check |
|---|---|
| A default or an activation rule stated too narrowly | read the source that decides it, all branches |
| An output line paraphrased | quote the line from the run |
| A test that passes without the feature | step 4 |
| A statement true for one case and written for all | name the cases; test the neighbours (a test fixture, a class fixture, an assembly fixture) |
| "Cannot be shown on this machine" that was never tried | try first; `browser-wasm` was claimed impossible and works |
| Evidence that belongs to another rule or another run | the command in the log names the rule or the test |
| Counts that went stale after a correction | step 6, last sentence |
| A row that credits the wrong thing | the row describes the API, and the API exists in `docs/api` |

## Done

- Every type, member, rule, property, key and option under Scope has a catalog row or stands under
  `Not shown` with a reason that was tested.
- `dotnet build "MSTest 4.4.1.slnx"` ends with 0 warnings and 0 errors.
- The ordinary run of every project ends with exit code 0; every `[Explicit]` demonstration ends as its
  summary says.
- The Native AOT executable runs and passes.
- The guardrail command prints nothing. `git diff --check` prints nothing.
- Every added line of `ref/delta/api.md` that belongs to a consumer-facing type is found in `API.md`.
- `README.md` holds the counts of the final full run and no other.

## What exists

The repository holds the first attempt: about 40 projects with about 300 examples, cut by changelog entry
and written before these conventions. It is raw material. Move what holds into the topic layout and
rewrite it to the conventions.

A review proved 62 errors that are still in that code: contradicted and unsupported statements, wrong
attributions, weak reasons, missing examples. They are listed at the end of this document. Correct each
one you carry over.

Trust none of it without step 3 and step 4.

## Boundaries

- Commit, push, publish and create the GitHub repository only when the owner says so.
- Install no workload and no global tool. Start no container and no service.
- Build and run one project at a time, with `DOTNET_PROCESSOR_COUNT=2`.
- Delete, move or overwrite a file that you did not create only after the owner's word.

## Known errors in the first attempt

What a review proved and nobody corrected, by area of the first attempt. An entry number counts the
rows of that area in the order its author listed them; the file name or the API in the text finds the place.

### assertions (4)

- **contradicted**, entries 9, 10, 11, 12, 13 (Spans/ContainmentTests.cs: Contains, DoesNotContain, ContainsAll, DoesNotContainAll, ContainsSingle on Span/Memory): observed: 'With MSTest 4.3.3 ... 88 errors in the file' / 'the file does not compile (88 errors, spans-433.log)' *Evidence or correction:* With MSTest 4.3.3 the current ContainmentTests.cs gives 83 errors, not 88. The rest holds: the file does not compile with 4.3.3.
- **contradicted**, entry 37 (Make whitespace-only differences visible in bounded Assert.AreEqual string comparison previews), note: The markers appear only when whitespace other than tab, line feed and carriage return takes part in the mismatch *Evidence or correction:* Whitespace markers (<space>, \uXXXX) appear only when such whitespace takes part in the mismatch; [[...]] markers in the previews also appear for non-whitespace mismatches when the two previews cannot share one caret column, for example tokens of different length.
- **contradicted**, entries 14, 15, 16: class summary of MSTest.Assertions 4.4.1/Patterns/StringPatternTests.cs: Assert.MatchesRegex and Assert.DoesNotMatchRegex build the regular expression of a pattern string once and reuse it for the same pattern under the same culture. *Evidence or correction:* They reuse the regular expression of a pattern string for the same pattern under the same culture while it stays in a bounded cache; a pattern longer than 512 characters, or one evicted by newer patterns, is built again.
- **contradicted**, entry 39 (MSTEST0032 enum member converted to its underlying type): class summary of MSTest.Assertions 4.4.1/Analyzers/EnumValueContractTests.cs: MSTEST0032 accepts this assertion and reports other comparisons of two constants *Evidence or correction:* MSTEST0032 accepts this assertion and reports the two other comparisons between the pragma lines; it does not report every comparison of two constants (Assert.AreEqual(1L, (long)DumpType.Mini) gets MSTEST0025 instead).

### data (2)

- **contradicted**, [CombinatorialData] (MSTest.Data 4.4.1/Combinatorial/CombinatorialDataTests.cs), observed: Ordinary run: 'total: 99 / failed: 0 / succeeded: 97 / skipped: 2', EXIT CODE 0. *Evidence or correction:* Ordinary run: 'total: 106 / failed: 0 / succeeded: 104 / skipped: 2', EXIT CODE 0.
- **unsupported**, [CombinatorialData]: NotSupportedException / ArgumentException rules (MSTest.Data 4.4.1/Combinatorial/ValueProviderRulesTests.cs), note: The whole project also passed under LANG=de_DE.UTF-8, ja_JP.UTF-8 and tr_TR.UTF-8. *Evidence or correction:* searched: scratch/data-verify/out/lang-de_DE.UTF-8.txt, lang-ja_JP.UTF-8.txt, lang-tr_TR.UTF-8.txt (all 18:58) and the refute log call #37 end with 'gesamt: 99' / '合計: 99' / 'toplam: 99', i.e. the project before the CombinatorialRandomDataTests correction. The current project has 106 tests. scratch/data/out/lang-after-en_US.UTF-8.txt holds only 'WAITING for a build slot', and the catalog's own doubts say the repeat…

### execution (5)

- **contradicted**, 4.4.0 Added: TestOutputCaptureMode ... (api: captureTrace Live with Console.WriteLine; verifiedBy LiveOutputTests.The_line_is_on_the_standard_output_before_its…: "Control on a scratch copy with captureTrace Result: all three rows fail with \"Test '...' timed out after 8000ms\"." *Evidence or correction:* Control on a scratch copy with captureTrace Result: all four LiveOutputTests fail with "Test '...' timed out after 30000ms"; total: 4, failed: 4; exit code 2.
- **unsupported**, notDemonstrated: 4.4.0 Fixed: live MSTest output deadlocking when test execution and output forwarding contend during completion (#10169): "a run with concurrent Console, Trace and TestContext output passes whether the fix is present or not" *Evidence or correction:* searched: logdigest --grep '10169|Concurrent|concurrent|deadlock|InvertConsoleWriterLocks' over all five execution logs (write, refute, fix round 1, fix, earlier audit): no run with concurrent live output on any version. No released package can show it either: Live mode (#10013/#10040) and the fix (#10169) both ship in 4.4.0 (mstest-changelog.md lines 59, 101), and 4.3.3 rejects the mode ('Invalid value 'Live' for r…
- **wrong attribution**, execution#13: since 4.4.0 credited to 4.4.0 Fixed #9701 (mstest-changelog.md:99), but the entry's own evidence shows identical behaviour on 4.3.3; the bullet therefore has no discriminating example and no notDemonstrated item *Evidence or correction:* observed: scratch/execution/v433 (MSTest.TestFramework/4.3.3) 'passed A_directory_without_an_entry_is_null' ... total: 7, succeeded: 7; note: 'TestContext.GetProperty is IL-identical between 4.3.3 and 4.4.1'
- **wrong attribution**, execution#14: same as execution#13 for TestContext.TestName / FullyQualifiedTestClassName: presented under the 4.4.0 fix, behaviour unchanged since 4.3.3 *Evidence or correction:* observed: 'Also passes on MSTest 4.3.3 (scratch/execution/v433)'; note: 'Current behaviour, not a verified 4.4.0 change'
- **wrong attribution**, execution#15: MSTEST0048 on string-keyed TestContext.Properties presented as the 4.4.0 extension of the fixture analyzer, but MSTest.Analyzers 4.3.3 already reports the same diagnostics *Evidence or correction:* observed: v433 build 'error MSTEST0048: TestContext property 'FullyQualifiedTestClassName' cannot be accessed in 'AssemblyInitialize' method' and the TestName error, '2 Error(s)'; note: 'TestContextPropertyUsageAnalyzer is identical in 4.3.3, 4.4.0 and 4.4.1 (diff exit 0)'

### isolation (3)

- **contradicted**, MSTEST0074 (MSTest.Isolation.ClassLevel 4.4.1/Analyzers/ProcessGlobalStateRuleTests.cs); the same activation is inherited by the MSTEST0075 note 'Same activati…: note: 'Reports only with [assembly: Parallelize] in source or mstest_parallel_safety_mode = always in .editorconfig' *Evidence or correction:* The rules report when the assembly declares [assembly: Parallelize], or when mstest_parallel_safety_mode = always is set in a .globalconfig, as an MSBuild build_property or in an .editorconfig section. In every case an [assembly: DoNotParallelize] turns them off (ParallelSafetyHelper.cs:57-97).
- **contradicted**, MSTEST0076 (MSTest.Isolation.ClassLevel 4.4.1/Analyzers/DefaultCultureRuleTests.cs): note: 'No code fix: the diagnostics were still there after dotnet format.' *Evidence or correction:* No code fix (docs/mstest/analyzers/mstest0076.md:12; AddResourceLockFixer.cs:38-41 fixes only MSTEST0074 and MSTEST0075). Drop the dotnet format sentence, because that run filtered to --diagnostics MSTEST0074 MSTEST0075.
- **weak reason**, notDemonstrated isolation#0 (TestTempDirectory across platforms): 'the browser-wasm and wasi-wasm branch needs the wasm workload' is false for browser-wasm on this machine (Microsoft.NET.Sdk.WebAssembly + bun, packaging#8); only the Windows and wasi-wasm branches are real limits.

### ordering (3)

- **contradicted**, 4.4.0 testconfig.json chains (mstest:execution:dependencies:chains), MSTest.Ordering 4.4.1/Configuration/ChainTests.cs, note: A filtered run prints one warning per configured declaration whose test is outside the filter: 'The 'mstest:execution:dependencies' configuration section declares dependencies for '...', which matches no test in this run. They are ignored.' *Evidence or correction:* A filtered run prints one warning per dependent/prerequisite pair (each link of a chain, each dependsOn entry of a node) whose dependent matches no test in the run; for this testconfig.json and a filter on another class that is 13 lines.
- **contradicted**, notDemonstrated: 4.4.0 [DependsOn] 'The attribute works on both hosts' (VSTest): 'dotnet vstest "artifacts/bin/MSTest.Ordering 4.4.1/debug/MSTest.Ordering 4.4.1.dll"' in that copy: 'Failed! - Failed: 7, Passed: 49, Skipped: 9, Total: 65'; evidence cites Sdk.targets lines 11 and 15-17 *Evidence or correction:* On the current code: 'Failed! - Failed: 8, Passed: 47, Skipped: 10, Total: 65', all 8 failures ordered only by testconfig.json, every test ordered by [DependsOn] passes; cite Sdk.targets lines 11, 15 and 18.
- **contradicted**, notDemonstrated: 4.4.0 'the testconfig.json declarations are Microsoft.Testing.Platform only': the seven failures are exactly the tests ordered only by testconfig.json ('Failed Load', 'Failed Transform', 'Failed Pack', 'Failed Analyze', 'Failed Build', 'Failed HomePage', 'Failed SignInPage') *Evidence or correction:* Under dotnet vstest the eight failures are exactly the tests ordered only by testconfig.json: Transform, Load, Pack, Publish, Analyze, Build, HomePage, SignInPage.

### packaging (3)

- **contradicted**, notDemonstrated: Cache immutable Microsoft.Testing.Platform node properties when converting MSTest test elements, since 4.4.0: Not written as an example because a test of the identity cannot be attributed to this entry and 'no public API or document promises the shared instances'. The entry is therefore listed as not demonstrable. *Evidence or correction:* Turn the Identity probe into an example (an IDataConsumer registered in Program.cs asserting ReferenceEquals of TestMethodIdentifierProperty/TestFileLocationProperty/TestMetadataProperty across the in-progress and passed updates of one test, and distinct TestNode/PropertyBag), since 4.4.0. Otherwise the reason has to say this is a scope choice and not a limit of this machine.
- **unsupported**, notDemonstrated: Prevent long-running .NET Framework tests ... (misclassification half), since 4.4.0: The misclassified form existed only between the two pull requests inside the 4.4.0 development, so no run with released packages can show it being classified differently. *Evidence or correction:* searched: Logs of write, refute and fix (logdigest --grep 'isclassif|StandardErrorWriter|MSBuild error|10604'): only the changelog line 103 and AssemblyResolver.Resolution.cs:218-246 are quoted. No log names what the misclassified form was. ref/testfx is a shallow clone at v4.4.1 ('git rev-parse --is-shallow-repository' = true), so the history of #10532/#10604 cannot be read. Supported part: MSTestAdapter.PlatformSe…
- **weak reason**, notDemonstrated packaging#1 (CloneWithUpdatedSource): The obstacle (only caller chain ends in the VSTest executor) is not a machine limit: dotnet vstest runs on this machine (notDemonstrated ordering#0: 'Failed: 7, Passed: 49, Skipped: 9, Total: 65'); the reason does not show the fix is unobservable, only that the repository's MTP runner does not reach it.

### platform-added (17)

- **contradicted**, IHierarchicalConfigurationProvider (entries[16]); class summary of MSTest.Platform 4.4.1/Configuration/ConfigurationSourceTests.cs: The children of a section are the keys of all sources that implement IHierarchicalConfigurationProvider, each key once and in alphabetical order. *Evidence or correction:* '... each key once, keys that are whole numbers first in numeric order (the items of an array), then the other keys in ordinal order without regard to case.'
- **weak reason**, notDemonstrated platform-added#2 (MTP execution on browser-wasm and wasi-wasm): Names an obstacle this machine does not have for browser-wasm: 'Needs the wasm-tools workload' and 'neither wasmtime nor node is installed'. MSTest.Packaging.Browser 4.4.1 and MSTest.Platform.Fixed.Browser 4.4.1 build with Sdk="Microsoft.NET.Sdk.WebAssembly" (shipped in ~/.dotnet/sdk/10.0.401/Sdks) without a workload and run under bun 1.4.2 (~/.bun/bin/bun): packaging#8 'MSTest.Packaging.Browser 4.4.1 (net10.0|brows…
- **weak reason**, notDemonstrated platform-added#14 (LaunchInProcessAsync PlatformNotSupportedException on browser/WASM): Same false obstacle ('the wasm-tools workload is not installed'); a browser-wasm application builds and runs here (packaging#8, platform-fixed#21-#23).
- **not covered**, mtp-changelog.md:59 Add artifact post-processing for dotnet test ... The built-in report processors do not opt in, and partially written or malformed artifacts…: 'partially written or malformed artifacts remain invalid inputs': no entry and no notDemonstrated item (grep 'malformed'/'partially written' over all fixed.json: 0 hits). platform-added#6/#7 and notDemonstrated platform-added#8-#10 cover the other parts.
- **not covered**, mtp-changelog.md:70 Pin GitHubActionsReport annotations to TestFileLocationProperty ... The property is populated by the MSTest adapter and by the VSTest bridg…: population of TestFileLocationProperty by the VSTest bridge (bridged frameworks). platform-added#24 shows only the MSTest adapter path; no notDemonstrated item.
- **API without example**, Microsoft.Testing.Extensions.AzureFoundry.TestApplicationBuilderExtensions (new overload AddAzureOpenAIChatClientProvider(ITestApplicationBuilder, TokenCredent…: Platform/Microsoft.Testing.Extensions.AzureFoundry/PublicAPI
- **API without example**, Microsoft.Testing.Platform.CommandLine.CommandLineOptionsExtensions (api.md:110, :357): Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, Microsoft.Testing.Platform.OutputDevice.ProgressMessageOutputDeviceData (api.md:198): Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, Microsoft.Testing.Platform.OutputDevice.SessionMessageOutputDeviceData (api.md:202): Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, [TPEXP] Microsoft.Testing.Platform.Capabilities.TestFramework.IGracefulStopTestExecutionResultCapability (api.md:242): Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, [TPEXP] Microsoft.Testing.Platform.Extensions.ArtifactPostProcessing.ArtifactPostProcessingTruncationReason (api.md:268): Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, [TPEXP] Microsoft.Testing.Platform.Extensions.ArtifactPostProcessing.IArtifactPostProcessorRequiresPostProcessing (api.md:280): Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, [TPEXP] Microsoft.Testing.Platform.Extensions.TestHostControllers.ITestHostControllerConnectionAuthorizer (api.md:299): Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, [TPEXP] Microsoft.Testing.Platform.Requests.CompositeTestExecutionFilter (api.md:301): Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, [TPEXP] Microsoft.Testing.Platform.Requests.TestExecutionFilterOperator (api.md:311): Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, [TPEXP] Microsoft.Testing.Platform.Tools.ITool, IToolCommandLineOptionsProvider, IToolsManager (with IArtifactPostProcessingApplicationBuilder.Tools) (api.md:2…: Platform/Microsoft.Testing.Platform/PublicAPI
- **API without example**, [TPINTERNAL] Microsoft.Testing.Platform.CommandLine.CommandLineOptionsProviderBase, Microsoft.Testing.Platform.Helpers.IClock (with ServiceProviderExtensions.G…: Platform/Microsoft.Testing.Platform/PublicAPI

### platform-changed (3)

- **contradicted**, Use controller-backed TRX recovery by default ... (#10808) - plain --report-trx (example MSTest.Platform.Changed 4.4.1/TrxRecovery/ControllerRunTests.cs, class…: "<c>--report-trx</c> is part of the default profile of MSTest.Sdk." *Evidence or correction:* The TRX report extension (Microsoft.Testing.Extensions.TrxReport, EnableMicrosoftTestingExtensionsTrxReport) is part of the Default profile of MSTest.Sdk, so --report-trx is available without extra setup; the option itself is not passed by default and has to be given on the command line.
- **wrong attribution**, platform-changed#1: since 2.4.0 for #9782 (CTRF existing-file behaviour, mtp-changelog.md:88), but the CTRF package paired with MSTest.Sdk 4.3.3 already warns and overwrites; the example cannot tell the pins apart *Evidence or correction:* observed: 'with MSTest.Sdk 4.3.3 (CTRF 1.0.0-alpha.26377.5) the same rows pass as well' (scratch/platform-changed-verify/v433-run.txt:45,49)
- **weak reason**, notDemonstrated platform-changed#11 (in-process TRX path on browser, WASI, iOS, tvOS): 'Needs a workload and another platform: browser-wasm' is contradicted by platform-fixed#21, which already runs --report-trx on browser-wasm here ('TRX Report: <dir>/notes.trx', EXIT CODE 0); that is the in-process path (TrxModeHelpers.cs:33-38).

### platform-fixed (13)

- **contradicted**, notDemonstrated #10014 (artifact merge output paths / case-sensitivity probe): The merge paths belong to the artifact post-processing of dotnet test ... dotnet test of SDK 10.0.401 runs no artifact post-processing. *Evidence or correction:* The merge helpers, and with them the case-sensitivity probe, also run in the Retry extension's post-processing after --retry-failed-tests. The item still belongs under notDemonstrated, but only because the defect and its fix are both in 2.4.0 (no released version has the defect), not because the path is unreachable.
- **contradicted**, notDemonstrated #10769 (untagged duplicate-UID CTRF results, collapse only unambiguous retry sequences): the collapsing of retry sequences (CtrfReportMerger.RetryCollapsing.cs) runs in the artifact post-processing, which dotnet test of SDK 10.0.401 does not run. *Evidence or correction:* Retry collapsing runs in an ordinary --retry-failed-tests run with --report-ctrf, and ConsolidatedReportRunTests already exercises it. The part that remains unshowable is the untagged duplicate-UID case, because MSTest tags its in-process retries.
- **contradicted**, notDemonstrated #10734 (preserve inputs when TRX or CTRF post-processing cannot create its merged output directory): Artifact post-processing is not run by dotnet test of SDK 10.0.401 (no 'internal-merge-artifacts'/'PostProcessor' in its dotnet.dll). *Evidence or correction:* The CTRF half of #10734 can be reached through --retry-failed-tests --report-ctrf; the post-processing directory is under Path.GetTempPath() (RetryOrchestrator.cs:446-448). Only the TRX half needs dotnet test post-processing. Nobody tried to make the merged-directory creation fail.
- **contradicted**, notDemonstrated #10963/#10997 command-line redaction for response-file diagnostics: The redactor masks only the dotnet test HTTP token and endpoint options, internal options of the browser dotnet test transport, and writes to the diagnostic log. *Evidence or correction:* Redaction can be observed on the output device. It is not observable only through the diagnostic log. The item can stay under notDemonstrated only because 4.4.0 prints the same redacted lines for top-level and nested response files, so no difference is visible. The redaction during the Retry relaunch was not probed.
- **contradicted**, Retries/ResponseFileRunTests (#10963, #10997), note and doubts: Not compared with 4.4.0 at run time. (doubts: '#10641, #10944, #10963/#10997 and #11037 were not compared at run time') *Evidence or correction:* Both 4.4.0 (RetryArgumentsBuilder.cs Line=34) and 4.3.3 (RetryOrchestrator.cs Line=107) were compared at run time and crash with EXIT CODE 134 on nested response files; 4.4.1 passes. Remove #10963/#10997 from the 'not compared' doubt.
- **contradicted**, Controller/EarlyExitRunTests (#11152), class summary: When the test host ends before it connects to the controller, the controller waits for the connection until the named-pipe connection timeout is over, prints the timeout and the exit code of the test host, and ends the run with exit code 1. *Evidence or correction:* Say: when the test host ends before it connects, the controller reports it and ends with exit code 1. If the host had already exited when the controller checked, this happens at once ('after '0' seconds'). Otherwise it happens after the named-pipe connection timeout (default 300 s, set with TESTINGPLATFORM_DEFAULT_HANG_TIMEOUT), and the message then names that timeout. The example shows the second case.
- **weak reason**, notDemonstrated platform-fixed#15 (command-line redaction for response-file diagnostics): States what the redactor masks but names no obstacle; --diagnostic works here (platform-fixed#7) and response files with --retry-failed-tests run here (platform-fixed#15), so why the masked diagnostic log line cannot be shown is not explained.
- **weak reason**, notDemonstrated platform-fixed#18 (named-pipe server disposal connection reset): 'no public API reaches it' is inaccurate: the controller named-pipe servers are started by public options the repository already uses (--report-trx under the test-host controller, platform-changed#29, platform-fixed#12/#18); evidence names a file without a line.
- **weak reason**, notDemonstrated platform-fixed#7 (TRX handshake race): Generic 'race ... would pass on the old version most of the time' with only the changelog line as evidence; no source line.
- **weak reason**, notDemonstrated platform-fixed#17 (self-wait disposing in-process client): Generic 'timing-sensitive' with only the changelog line as evidence; no source line.
- **not covered**, mtp-changelog.md:111 Fix --report-trx crashing with PlatformNotSupportedException on single-threaded WebAssembly runtimes (browser-wasm / wasi-wasm): wasi-wasm half: platform-fixed#21 shows browser-wasm only; no notDemonstrated item in platform-fixed names wasi-wasm for this bullet.
- **not covered**, mtp-changelog.md:113 Report test and session file artifacts through the simplified console output used by browser-wasm and wasi-wasm: wasi-wasm half: platform-fixed#22 runs MSTest.Platform.Fixed.Browser 4.4.1 (browser-wasm) only; no notDemonstrated item for wasi-wasm.
- **not covered**, mtp-changelog.md:116 Fix Microsoft.Testing.Extensions.AzureDevOpsReport throwing PlatformNotSupportedException on browser-wasm (and wasi-wasm): wasi-wasm half: platform-fixed#23 shows browser-wasm only; no notDemonstrated item for wasi-wasm.

### reporting (5)

- **contradicted**, assert.expected / assert.actual of a failed test node in server mode (the IDE path) [4.4.0, #10353], note; repeated in doubts: Source: src/Platform/Microsoft.Testing.Platform/ServerMode/JsonRpc/SerializerUtilities.TestNodeSerializers.cs:228-237 writes AssertionFailureProperty.Expected/Actual ?? string.Empty, and without the property the Exception.Data fallback *Evidence or correction:* Cite SerializerUtilities.TestNodeSerializers.cs:202-213 (property at 207-208, Exception.Data fallback at 212-213; the same logic in ServerMode/JsonRpc/Json/Json.TestNodeSerializer.cs:141-152), in the entry note and in the doubt.
- **contradicted**, notDemonstrated: Emit OpenTelemetry spans for MSTest tests and fixture methods (#10358) - throwing [AssemblyInitialize]/[AssemblyCleanup]; also rootRequests: A throwing [AssemblyInitialize] or [AssemblyCleanup] is not shown ... LifecycleTests holds them and they must succeed ... It needs a test assembly of its own. / rootRequests: 'It cannot live in MSTest.Reporting 4.4.1' *Evidence or correction:* Not a limitation: the case can be demonstrated in MSTest.Reporting 4.4.1 by gating a throw in LifecycleTests.OpenTheShop (and CloseTheShop) on an environment variable that only a run test sets for its nested process, then asserting 'MSTest.AssemblyInitialize' status Error with an exception event and no MSTest.TestMethod span. Either add that example or restate the reason as a design choice; the rootRequest for a sep…
- **contradicted**, Example file MSTest.Reporting 4.4.1/Spans/LifecycleTests.cs (class summary; example of the MSTest.TestMethod and fixture span entries): an ignored test does not run and has no span *Evidence or correction:* An ignored test has no MSTest.TestMethod span (and no fixture spans); the platform still records its test-case span with test.case.result.status 'skipped'.
- **contradicted**, Example/verifying file MSTest.Reporting 4.4.1/Spans/UnsuccessfulRunTests.cs (class summary; verifies the throwing class initialize entry): a class initialize method that throws leaves the tests of its class without a span *Evidence or correction:* A class initialize method that throws leaves the tests of its class without an MSTest.TestMethod span; the failure appears on the platform's test-case span of each test (status Error, with the exception message), not on any MSTest.* span.
- **API without example**, Microsoft.Testing.Extensions.TrxReport.Abstractions.TrxWorkItemsProperty (api.md:99): Platform/Microsoft.Testing.Extensions.TrxReport.Abstractions/PublicAPI

### retry (2)

- **contradicted**, Report every [Retry] attempt to the test host: CTRF report detects 'flaky' with 'retries' / 'retryAttempts[]' (--report-ctrf): note: 'The options are older than 4.4.0 (ref/mstest-docs/docs/testing-platform/features/test-reports.md lines 95-97 and 121, "Available in MTP starting with version 2.3.0")' *Evidence or correction:* Cite test-reports.md line 118 (CTRF section) for 'Available in MTP starting with version 2.3.0'; lines 131-132 (options table) and line 136 (retry paragraph 'records earlier attempts in retryAttempts, sets retries, and marks a later successful result as flaky: true') for the retry data.
- **contradicted**, Report every [Retry] attempt to the test host: superseded attempts are filtered out of the JUnit report (--report-junit): note: 'test-reports.md lines 95-97 "Available in MTP starting with version 2.3.0", line 121 "Starting with MSTest.Sdk 4.3" for the property' *Evidence or correction:* Cite test-reports.md line 95 for 'Available in MTP starting with version 2.3.0' and line 97 for 'Starting with MSTest.Sdk 4.3' and the property.

### sourcegen (2)

- **wrong attribution**, sourcegen#20: [AssemblyInitialize] listed with kind 'attribute', since 4.4.0 and an empty note; the attribute is absent from the api.md added lines (exists in 4.3.3) and its verifier (Assembly_initializer_of_the_project_runs) passes independent of MSTEST0072 *Evidence or correction:* api.md TestFramework added lines (399-664) contain no AssemblyInitialize; entry observed: 'Passed under dotnet run and in the native executable'
- **wrong attribution**, sourcegen#21: [DynamicData] (not in the api.md added lines, exists in 4.3.3) listed as kind 'attribute' since 4.4.0; the note claims 'No released predecessor of this generator exists to compare warnings with', contradicting notDemonstrated sourcegen#0 which names the released predecessor MSTest.SourceGeneration 2.0.0-alpha.26264.3 restored by MSTest.Sdk 4.3.3 *Evidence or correction:* sourcegen/fixed.json entries[21].note vs notDemonstrated[0].reason
