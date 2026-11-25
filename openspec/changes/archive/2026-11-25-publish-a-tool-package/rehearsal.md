# Rehearsal

Run on 2026-11-25 on this machine, .NET SDK 10.0.404, Windows x64. Nothing was pushed
to nuget.org; the feed was a folder.

## What was rehearsed

Pack the tool, install it from a folder feed as a user would install it from nuget.org,
start the tray, uninstall it, and confirm the package carries the version the tag will
name and nothing it should not.

## Pack

```
dotnet pack src/Picadence/Picadence.csproj -c Release -p:Version=0.4.0 -o .tmp/rehearsal/nupkg
```

`Successfully created package '...\Ypyl.Picadence.0.4.0.nupkg'`, 9 seconds including the
NativeAOT publish. The package is **1,011,795 bytes**.

Contents, as listed from the archive:

| Entry | Bytes |
|---|---|
| `README.md` | 12,054 |
| `Ypyl.Picadence.nuspec` | 782 |
| `tools/any/win-x64/DotnetToolSettings.xml` | 198 |
| `tools/any/win-x64/Picadence.exe` | 1,951,232 |
| `_rels/.rels`, `[Content_Types].xml`, `package/services/...` | metadata |
| **`.pdb` files** | **0** |

The first pack of the same change carried `Picadence.pdb` at 10,711,040 bytes, because
the publish folder still held a symbol file from an earlier build and the tool layout
copies that folder as it finds it. Two changes came out of it: `DebugType` is `None`
for a Release build, so no symbols are produced, and the release workflow greps the
package for a `.pdb` and fails if there is one. With `bin/` removed the pack above is
what a fresh checkout produces.

The nuspec says `id Ypyl.Picadence`, `version 0.4.0`, `license expression MIT`,
`readme README.md`, `packageType DotnetTool`, and the repository commit. The tool
settings say `Command Name="picadence" EntryPoint="Picadence.exe" Runner="executable"`.

## Install, run, remove

```
dotnet tool install --tool-path ./tools --add-source ./nupkg Ypyl.Picadence --version 0.4.0
```

One second. Both lines a reader checks were printed: `You can invoke the tool using the
following command: picadence` and `Tool 'ypyl.picadence' (version '0.4.0') was
successfully installed.` The shim it wrote is a batch file:

```
@echo off
"%~dp0.store\ypyl.picadence\0.4.0\ypyl.picadence\0.4.0\tools\any\win-x64\Picadence.exe" %*
```

Starting that shim put `Picadence.exe` in `tasklist` at 13,076 K working set, the same
figure the exe shows when it is started on its own. It was stopped with `taskkill` after
three seconds.

```
dotnet tool uninstall --tool-path ./tools Ypyl.Picadence
```

`Tool 'ypyl.picadence' (version '0.4.0') was successfully uninstalled.`, and the tool
path is empty afterwards.

## What this does not rehearse

The feed. Whether nuget.org accepts the package, and whether the trusted publishing
policy hands out a key, can only be seen by pushing. The first real publish is watched
for that reason, and the Deploy block's four lines are run for it.
