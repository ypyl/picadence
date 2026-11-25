# The evidence is on the page (L2)

## Why

`adr/0007-the-gate-carries-the-evidence.md` records the decision. The short version: an
L3 gate could not show a change's rehearsal, rollback, or reconciliation, because
`scripts/gate.mjs` collects only proposal, design, tasks, specs, and the ADR. The first L3
in the repository proved it on 2026-11-25, when the evidence page carried two files and
the evidence was invisible.

## What Changes

- `collectArtifacts` reads `rehearsal.md`, `rollback.md`, and `reconciliation.md` as
  artifacts of kind `evidence`, hashed like every other artifact.
- `arrange` puts evidence after the decision and before the design, so a page reads
  decision, evidence, then detail.
- `selectForGate` gains an `evidence` gate: the evidence files plus the proposal as
  context.
- The CLI accepts `--gate evidence`, and the help names it.
- After an ADR gate on a change that has evidence and no delta spec, the tool prints the
  next gate as `--gate evidence`.
- `AGENTS.md`'s L3 line names that second gate instead of "gated the same way".
- Tests: the three files are collected with kind `evidence`; the evidence gate selects
  them and the proposal and nothing else; the page for that gate carries each hash; and a
  change without evidence still opens its ADR gate as before.

## Capabilities

None. No product requirement moves; this is the tool that enforces the process, so
`skip_specs: true`.

## Impact

- `scripts/gate.mjs`, `scripts/gate.test.mjs`, and one sentence in `AGENTS.md`.
- Every later L3 gate carries the evidence and its hashes.

## Routing

0. **Does observable behavior change?** Yes, in the tool: an L3 evidence page shows
   three files it could not show before, and its approval names them. The product is
   untouched.
1. **Can it be undone easily?** Yes, one commit; no data and nothing stored outside the
   repository.
2. **Can a machine prove it correct?** Yes: `scripts/gate.test.mjs` runs in
   `scripts/verify.sh` on every change, and the new tests fail before this change
   because the files are never collected.
3. **Does anything outside the change depend on it?** The `AGENTS.md` gate section holds
   the contract, and it moves with this change. `adr/0007` holds the decision.

Level: **L2**. One gate, the ADR, because `skip_specs: true` means there is no delta spec
to gate second.

## What this unblocks

`publish-a-tool-package` waits on branch `change/publish-a-tool-package` at revision 1.
When this lands, that change is re-gated at revision 2 with its evidence on the page.

## Approval

- Gate: L2 ADR
- Approved by: Yauhen Pyl
- Approved at: 2026-11-25
- Artifact: adr/0007-the-gate-carries-the-evidence.md sha256:a9f015a5da0d8989e807bbba9af52de186b4da1a6505131fc5ec2a0b8ad119c5
- Artifact: openspec/changes/the-evidence-is-on-the-page/proposal.md sha256:683577b2a3e4521ec14fd972bdcd82a5cfc65a8c2cfcf704091a81e0f2728bd5
- Words: "(none)"
