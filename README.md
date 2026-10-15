# MSTest examples

The projects use MSTest SDK, framework and adapter **4.5.1** and Microsoft.Testing.Platform
**2.5.1** on .NET SDK **10.0.401**. `global.json` pins the MSTest SDK; its defaults select
the matching framework, adapter and platform. Explicit package references use those same versions.
The `LegacyBase` analyzer fixture deliberately keeps MSTest 3.11.1. Project and folder names retain
`4.4.1` so existing paths stay usable.

Run an ordinary project through the native MTP driver:

```sh
DOTNET_PROCESSOR_COUNT=2 dotnet test --project "MSTest.Data 4.4.1/MSTest.Data 4.4.1.csproj"
```

Run the retry examples through their MTP executable:

```sh
DOTNET_PROCESSOR_COUNT=2 dotnet run --project "MSTest.Platform.Changed 4.4.1/MSTest.Platform.Changed 4.4.1.csproj" -- --report-trx
```

With .NET SDK 10.0.401, the native `dotnet test` driver counts a superseded failed `[Retry]`
attempt as a failure even when the final attempt passes. The MTP executable reports the retry
history, and its TRX report records the final outcome.

The two `browser-wasm` projects run under Bun through their configured `dotnet run` launcher:

```sh
DOTNET_PROCESSOR_COUNT=2 dotnet run --project "MSTest.Packaging.Browser 4.4.1/MSTest.Packaging.Browser 4.4.1.csproj"
DOTNET_PROCESSOR_COUNT=2 dotnet run --project "MSTest.Platform.Fixed.Browser 4.4.1/MSTest.Platform.Fixed.Browser 4.4.1.csproj"
```

The native driver in this SDK cannot launch these browser modules. If a browser bundle still loads
the previous package version after an upgrade, regenerate it with
`dotnet build "path/to/project.csproj" --no-restore -t:Rebuild`.

`MSTest.Platform.Extension` is a plugin DLL loaded by the test host. Its explicit
`IsTestingPlatformApplication=false` overrides MTP's default of `true`, so the solution runner
classifies it as a library. `DynamicExtensionRunTests` exercise it through the executable host.

On this Mac, the dotfiles repository provides installation checks and Rider-compatible inspections:

```sh
make -C ~/.local/share/chezmoi dotnet-tools dotnet-testing
make -C ~/.local/share/chezmoi inspectcode SOLUTION="$PWD/MSTest-4.4.1.slnx" NO_BUILD=1
```

InspectCode includes INFO, HINT, SUGGESTION, WARNING and ERROR findings and writes
`artifacts/inspectcode.sarif` using the solution's shared Rider/ReSharper and EditorConfig settings.
