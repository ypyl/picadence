# 0006. The tray also ships as a .NET tool on nuget.org

- Status: Accepted
- Date: 2026-11-25

## Context

The product installs one way today: a 1.95 MB NativeAOT exe from the release page or
from winget, with no runtime and no installer, plus a pi package for the half that
reports state. ADR 0003 keeps every push on a runner. The Deploy block in `AGENTS.md`
is empty, so a release is a tag and a hand upload.

A person who lives in a terminal asked for `dotnet tool install`, the way they install
every other small utility. .NET 10 supports RID-specific tools: `dotnet pack` with
`PackAsTool` produced `tools/any/win-x64/Picadence.exe` from the same AOT publish, and
`dotnet tool install` put a `picadence.cmd` shim in the tool path. Measured on this
machine on 2026-11-25: a 1.95 MB exe in the package plus a 10.7 MB pdb, an install that
took a second, and the tray running from the package store at 13 MB working set.

The tool channel cannot replace the exe. The exe needs nothing; the tool needs the
.NET SDK, because `dotnet tool` is an SDK command. The shim is a console-subsystem
batch file that waits for the tray, so a Startup shortcut to it shows a console window
for the life of the process, which is the one autostart this product has.

A version published to nuget.org cannot be withdrawn or reused. It can be deleted
within 72 hours, and only unlisted after that, and the id and version stay spent. That
is why this decision is routed L3.

## Decision

We will publish the same NativeAOT build as a .NET tool package, `Ypyl.Picadence`, on
nuget.org, as a second channel for developers, and keep the single-file exe and winget
as the channel the README leads with.

Five constraints, part of the decision:

1. **One binary, two wrappers.** The package carries the publish output of the same
   project, so no code path is built twice and nothing is compiled only for NuGet.
2. **The version comes from the tag.** The release workflow derives the package version
   from `picadence-v<semver>`, so the feed and the tag cannot disagree.
3. **No long-lived key.** nuget.org trusted publishing exchanges a GitHub OIDC token for
   a key that lives an hour, so the repository gains no secret and Project facts keeps
   "Secrets: none, and none planned".
4. **The exe stays the product's install.** The README says which channel is for whom,
   and the tool route names the SDK as its prerequisite.
5. **A published version is permanent.** A bad release is unlisted and replaced by the
   next version, never reused and never republished.

## Consequences

- A developer with the .NET SDK installs `dotnet tool install -g Ypyl.Picadence` and
  gets `picadence` on the path. A user without the SDK loses nothing.
- The repository gains a release workflow, a package id to own, a trusted publishing
  policy on nuget.org, and one more step in the release checklist.
- The tool is win-x64 only, because the package is RID-specific. ARM64 Windows keeps
  the exe.
- The debug symbols stay out of the package; they are five times the exe.
- Rollback is unlist plus a new version, and the Deploy block records that command so
  it is not invented during an incident.

## Alternatives

- **Only the exe and winget**: no NuGet account and no second artifact. Rejected because
  the request is a terminal-native install, and the SDK is a pi user's tool already.
- **A framework-dependent tool package**: smaller and portable to ARM64, but it needs a
  .NET runtime on the user's machine, starts slower, and still carries the console shim.
- **A thin tool that downloads and installs the exe**: one install path, a CLI for setup,
  at the cost of a fetch inside the installer and an update path of its own. Deferred,
  not rejected: it is the shape to revisit if the console shim ever becomes the problem.
- **A NuGet API key in a repository secret**: one line in the workflow. Rejected because
  trusted publishing stores no credential at all.
- **`Picadence` as the package id**: it matches the command name. Rejected so the package
  id, the winget id, and the release tags follow one name. The nuget.org profile is
  `y_pyl`, which is the account, not the package.
