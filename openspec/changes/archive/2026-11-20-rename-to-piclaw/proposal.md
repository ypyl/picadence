# The product is called piclaw (L0)

## Why

The working name `trayclaw` was a placeholder. The product is a pi status panel
in the tray, and the name should say so: piclaw, pi plus claw. This renames the
product everywhere it appears: the slug, the assembly and exe, the namespace, the
extension file, the state directory, and the documents.

## What Changes

- Product slug `piclaw`, exe `Piclaw.exe`, namespace `Piclaw`, project files
  `Piclaw.csproj` and `Piclaw.Tests.csproj`, solution `piclaw.sln`.
- The pi extension becomes `extensions/piclaw.js`, and the package that loads it
  is reinstalled from the new name.
- Agent state moves to `%LOCALAPPDATA%\piclaw\agents`. The old directory holds
  ephemeral files and is deleted; the extension rewrites them within a second.
- The window classes, the tooltip, and the panel title say piclaw.
- The archive keeps its history: changes archived before this one still say
  trayclaw, because that is what the record said at the time.

No requirement moves. The requirement this leaves intact is every one in
`openspec/specs/agent-status/spec.md`: the product behaves exactly as before, and
only its name changes.

## Capabilities

- **New**: none. `skip_specs` is set.
- **Modified**: none. The spec never named the product, so nothing in it changes.

## Impact

- Every source file, both project files, the solution, the extension and its
  test, and the documents that name the product.
- This machine: the running tray is stopped, the old pi package entry is removed,
  the repository folder is renamed, and the extension is installed again from the
  new path.
- Not changed: the durable spec, which never named the product, and the archived
  changes, which are the record of what was decided then.
- ADR 0001 still says `%LOCALAPPDATA%	rayclaw`: its hash is part of the
  bootstrap approval, and an approved artifact is not edited. The current path
  lives in `ARCHITECTURE.md` and in the code, and a later ADR is the right place
  to restate it if anyone needs one.

## Routing

0. **Does observable behavior change?** No requirement moves. A name is copy.
1. **Can it be undone easily?** Yes, one commit, plus one install command on this
   machine.
2. **Can a machine prove it correct?** Yes: the build and the tests carry the new
   names, and a search for the old name outside the archive returns nothing.
3. **Does anything outside the change depend on it?** The pi package entry on
   this machine points at the old path, so this change reinstalls it in the same
   session. No document outside the repository holds the name.

Level L0: copy and build configuration, undone by one commit, proven by the
build, the tests, and a search.
