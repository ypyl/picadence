# The product is called picadence (L1)

## Why

Two reasons, one of them not a matter of taste.

`piclaw` is already the name of a pi project by another author (`rcarmo/piclaw`,
"pi coding agent in a technicolor web trenchcoat"), so publishing this under that
name attaches it to someone else's work.

And the name does not say what this is. `claw` has meant nothing since the
repository was `trayclaw`, and the product has outgrown "status mirror": it
watches every session and it wakes the ones you armed. `cadence` is already the
product's own word, in the spec, in the panel, and in every commit of the last
three changes.

## What Changes

- The product, the executable, the solution, the namespaces, the pi extension and
  the tag prefix become `picadence`.
- The local state directory becomes `%LOCALAPPDATA%\picadence`.
- The wake-up message says `picadence ping` instead of `piclaw ping`, and the
  panel title reads `picadence`.
- The two durable specs change the product name in their requirement text. One
  requirement and its scenario are renamed as a block, because the name is in
  their titles and a scenario title is the name of a test.
- **BREAKING** for the copy: anything a person has learned to read as `piclaw`
  now reads `picadence`. Nothing else about the behavior moves.

## Capabilities

### Modified Capabilities

- `agent-status`: two requirements name the product in their text, the panel's
  controls and the Start with Windows control. Same behavior, new name.
- `scheduled-ping`: the requirement that says the wake-up comes from the product
  is renamed with its scenario, and its text names the new product.

## Impact

- `AGENTS.md` Product slug, `README.md`, `CHANGELOG.md`, `DESIGN.md`,
  `ARCHITECTURE.md`, `REPO-FACTS.md`, `CONVENTIONS.md`.
- `piclaw.sln`, `src/Piclaw`, `tests/Piclaw.Tests`, every namespace.
- `extensions/piclaw.js` and its test file.
- `%LOCALAPPDATA%\piclaw`, which the code stops reading and writing.
- Accepted ADRs keep their wording, including the old directory name: an
  accepted decision is history and is not rewritten to say something else.

## Routing

0. **Does observable behavior change?** Yes, the words a person reads: the panel
   title, the wake-up message, the executable, the task in Task Manager. No
   requirement's meaning moves, so this is the smallest kind of behavior change
   and still not nothing.
1. **Can it be undone easily?** Yes, one commit reverts it. The old directory is
   left in place rather than deleted, so nothing is lost either way.
2. **Can a machine prove it correct?** Yes: the suite is renamed with the code and
   runs, the interop test asserts the window class name, the scheduled-ping test
   asserts the exact message text, and `scripts/verify.sh` builds from the
   renamed solution.
3. **Does anything outside the change depend on it?** Yes, the durable specs name
   the product, so the specs move with it, and so do the file contract the two
   halves share and the tag prefix a release will use.

Level: **L1**, because a requirement's text changes and a product owner should
read the new words before the code says them.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-21
- Artifact: openspec/changes/rename-to-picadence/specs/agent-status/spec.md sha256:0ebc9bb48d05eaaef3267a3ecd5cb6bb6e6d7da726b778c1d930612e169b8a79
- Artifact: openspec/changes/rename-to-picadence/specs/scheduled-ping/spec.md sha256:0859c483c9089368f42af9b2cfa61df7ac168e91c4f322c1172ffc376b9a40a9
- Artifact: openspec/changes/rename-to-picadence/proposal.md sha256:a2e4075ae683650ade967574e489fc44c6822dfeceb14d5f307f1fbefef252ae
- Words: "(none)"
