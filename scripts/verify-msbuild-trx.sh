#!/usr/bin/env bash
set -euo pipefail
root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
results=${1:-"$root/artifacts/msbuild-trx"}
mkdir -p "$results"
results=$(cd -- "$results" && pwd)
# MTP2.5.1 currently returns1 after all three tests pass: its controller host loses MSBuild IPC.
# Keep a failing exit as a failure; this check must pass after the framework root fix.
MSTEST_SHOWCASE_BUILD_SENTINEL=00000000000000000000000000000001 dotnet build \
  "$root/MSTest.Platform.TestTarget 4.5.1/MSTest.Platform.TestTarget 4.5.1.csproj" \
  --no-restore -t:Test -m:2 -p:TestingPlatformCaptureOutput=false \
  "-p:TestingPlatformCommandLineArguments=--report-trx --report-trx-filename msbuild.trx --results-directory $results"
test -s "$results/msbuild.trx"
