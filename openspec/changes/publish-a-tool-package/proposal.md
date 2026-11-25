# A tool package on nuget.org (L3)

## Why

The product installs one way: a 1.95 MB NativeAOT exe from the release page or winget,
with no runtime and no installer. That is right for the tray, and it is wrong for the
person who asked to install it with `dotnet tool install` and get `picadence` on their
path.

.NET 10 supports RID-specific tools, so the same AOT publish can be a tool package with
no second code path. Measured here on 2026-11-25: the package holds the 1.95 MB exe,
installs in a second, and the tray runs from the package store at 13 MB working set. The
tool also cannot replace the exe, because it needs the .NET SDK and its shim is a
console batch file that would put a window behind the tray started at login.

The decision is `adr/0006-a-tool-package-on-nuget.md`. It is L3 because a version
published to nuget.org cannot be withdrawn or reused.

## What Changes

- `Picadence.csproj` packs as a tool: `PackAsTool`, the command name `picadence`, the
  package id `Ypyl.Picadence`, a description, the MIT license expression, the repository
  URL, a packaged readme, and the debug symbols left out of the package.
- `.github/workflows/release.yml` starts on a `picadence-v*` tag, runs
  `scripts/verify.sh`, takes a one-hour key with `NuGet/login@v1` and `id-token: write`,
  packs with the version taken from the tag, and pushes to nuget.org. It runs in the
  `release` environment, so the repository can require a reviewer before a publish.
- `ci.yml` gets back the guarantee that a tag starts no ci run: `branches: ['**']` under
  `push`, because removing the branch filter made tags trigger it too.
- `README.md` gains the tool install line, the SDK prerequisite, and which channel is for
  whom. `AGENTS.md` gains the package id in Project facts and the four-line Deploy block.
- The first release that carries this is `picadence-v0.4.0`, which publishes
  `Ypyl.Picadence` 0.4.0.

## Capabilities

None. No requirement moves: the tray does the same thing however it was installed, so
`skip_specs: true`.

## Impact

- `Picadence.csproj`, a new `release.yml`, `ci.yml`, `README.md`, `AGENTS.md`,
  `adr/0006`, and the repository variable `NUGET_USER` (`y_pyl`).
- The package id is claimed by the first push, and no version can be reused after that.
- The human's side: a nuget.org account with 2FA and one trusted publishing policy
  (repository `ypyl/picadence`, workflow `release.yml`, environment `release`).

## Routing

0. **Does observable behavior change?** No. A second way to install the same program,
   which then behaves as it does today.
1. **Can it be undone easily?** No. A published version stays spent. The code can be
   reverted in one commit, but the package cannot be taken back.
2. **Can a machine prove it correct?** Yes, in the rehearsal: pack, install from a local
   folder feed, start the tray, uninstall, and check that the version in the package
   matches the tag. What no machine can prove is that the feed accepted it, so the first
   real publish is watched.
3. **Does anything outside the change depend on it?** Yes: nuget.org's trusted publishing
   policy and the package id itself are contracts outside this repository. That is the
   second reason this is L3.

Level: **L3**. Two gates: the ADR, then the evidence (`rehearsal.md`, `rollback.md`,
`reconciliation.md`).

## Evidence this change will carry

- `rehearsal.md`: the local-feed install, the run, the uninstall, and the version check,
  with the numbers.
- `rollback.md`: the local feed's package removed, the install that then fails, and the
  exact nuget.org command and its 72-hour limit.
- `reconciliation.md`: one version in the feed, one file, one hash, and the count of
  packages the feed lists.

## Approval

- Gate: L3 ADR
- Approved by: Yauhen Pyl
- Approved at: 2026-11-25
- Artifact: adr/0006-a-tool-package-on-nuget.md sha256:6c6d144d7c048a2efeb5efda9d5fc2756409031a02869da654b4ea972bf3dcc8
- Artifact: openspec/changes/publish-a-tool-package/proposal.md sha256:f49abfdd463017bd2ff86de14911083272fac4e6e56dddd5dcfc7ed41cffe333
- Words: "(none)"
