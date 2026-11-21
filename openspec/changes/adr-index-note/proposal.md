# The ADR index tells the truth about the name (L0)

## Why

Two things in `adr/README.md` are wrong. ADR 0002 was approved, and the index
still lists it as `Proposed`. And both accepted ADRs name the product as it was
called when they were written, `trayclaw` in 0001 and `piclaw` in 0002, which
reads as a different product to anyone who arrives after the rename.

The bodies stay exactly as they are. An accepted decision is the record of what
was decided then, and rewriting one to use today's name would make the decision
say something it never said.

## What Changes

- The index row for 0002 reads `Accepted`, which is what the gate recorded.
- The index carries one sentence above the table: the product was called
  `trayclaw`, then `piclaw`, and is `picadence` since 2026-11-21, so the names in
  the decisions below are the names of the day.

Nothing else. No requirement moves and no code changes.

## Capabilities

None. Documentation, so `skip_specs: true`.

## Impact

- `adr/README.md`.

## Routing

0. **Does observable behavior change?** No.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: `scripts/check-change.sh` treats
   `adr/README.md` as a contract document and `scripts/verify.sh` must stay
   green; the status is checkable against the ADR header.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
