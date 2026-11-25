# 0007. The gate carries a change's evidence

- Status: Accepted
- Date: 2026-11-25

## Context

`AGENTS.md` routes L3 through three artifacts: the rehearsal, the executed rollback, and
the reconciliation, "gated the same way" as a spec. `scripts/gate.mjs` collects five kinds
of artifact: `proposal.md`, `design.md`, `tasks.md`, anything under `specs/`, and the ADR
a proposal names. The three evidence files are none of those, so an L3 evidence page
carried the ADR and the proposal and nothing the gate was opened for.

The first L3 in this repository, `publish-a-tool-package`, hit it on 2026-11-25. The
rehearsal, the rollback and the reconciliation were written, run, and committed, and the
page that was supposed to show them listed two files.

That matters because the tool is the record. An approval names what it read by hash and
`--verify` recomputes it. A page that cannot show the evidence produces an approval that
names no evidence, which is approving on trust.

## Decision

We will treat `rehearsal.md`, `rollback.md`, and `reconciliation.md` as artifacts of a
kind called evidence, and add an `evidence` gate that carries them with the proposal as
context, the way the ADR and spec gates carry theirs.

Five constraints, part of the decision:

1. **Three names, not "any file".** A change folder that gates whatever someone drops
   into it turns scratch notes into approved artifacts and nothing is ever removable.
2. **The files stay the record.** No summary in the proposal and no copy in the page
   only: the evidence is hashed where it is written.
3. **Evidence is hashed like everything else**, so an evidence file edited after its
   approval fails `--verify` exactly as an edited spec does.
4. **The tool names the next gate.** After an ADR gate passes for a change with evidence
   and no delta spec, it says `--gate evidence`, because a page showing the decision and
   the evidence together would let one click approve both.
5. **Nothing in the tool runs the evidence.** It cannot know whether a command was run;
   it can only refuse to accept words that were never shown.

## Consequences

- An L3 with `skip_specs` has two pages: the ADR, then the evidence. `--gate all` stays
  for re-reading.
- `publish-a-tool-package` is re-gated at revision 2 with the evidence on the page, and
  its approval carries the three hashes.
- The three files appear in `--gate all` and in the archived approval block.

## Alternatives

- **Put the evidence in `design.md`**: no tool change, but the evidence then has two
  homes, or the three files the L3 archive check requires become pointers and the check
  and the record disagree about where the evidence lives.
- **Gate every file in the change folder**: no list to maintain, and scratch notes become
  approved artifacts.
- **Approve on trust**: the page stays as it is and the human reads the evidence in an
  editor. Rejected because an approval in the repository is a hash, and a page that cannot
  show a file cannot record its hash.
