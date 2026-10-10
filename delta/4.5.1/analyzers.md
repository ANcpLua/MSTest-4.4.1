# Analyzer rules: MSTest 4.4.1 → 4.5.1

Microsoft.Testing.Platform 2.4.1 → 2.5.1. Source: [microsoft/testfx](https://github.com/microsoft/testfx), tags `v4.4.1` and `v4.5.1`; the changelogs come from `main`.
Rows of `AnalyzerReleases.*.md` that the older tag does not have.

### `Analyzers/MSTest.Analyzers`, , New Rules

| Rule | | | |
|---|---|---|---|
| MSTEST0084 | Usage | Info | OSPlatformAttributesShouldBeConsistentAnalyzer, [Documentation](https://learn.microsoft.com/dotnet/core/testing/mstest-analyzers/mstest0084) |
| MSTEST0085 | Usage | Info | TestClassAttributeShouldNotBeAppliedToAbstractClassAnalyzer, [Documentation](https://learn.microsoft.com/dotnet/core/testing/mstest-analyzers/mstest0085) |
| MSTEST0086 | Usage | Info | RedundantTestMethodAttributeAnalyzer, [Documentation](https://learn.microsoft.com/dotnet/core/testing/mstest-analyzers/mstest0086) |
| MSTEST0087 | Usage | Warning | DuplicateDataRowDisplayNameAnalyzer, [Documentation](https://learn.microsoft.com/dotnet/core/testing/mstest-analyzers/mstest0087) |
| MSTEST0088 | Usage | Error | TestClassConstructorShouldBeValidAnalyzer, [Documentation](https://learn.microsoft.com/dotnet/core/testing/mstest-analyzers/mstest0088) |
| MSTEST0089 | Usage | Warning | ConflictingDataRowDisplayNamesAnalyzer, [Documentation](https://learn.microsoft.com/dotnet/core/testing/mstest-analyzers/mstest0089) |
