# The unreleased list reads as one list (L0)

## Why

Nine changes were written today, and each one put its entry at the top of
`## Unreleased`. Every prepend left the blank line that separates an entry from the
heading behind it, so the section now reads as five loose groups with stray gaps, and
there are two blank lines before `picadence-v0.3.0`.

A changelog is read by someone who does not read code. The released sections below it
are tight lists, so the difference reads as a mistake rather than a style.

## What Changes

- `## Unreleased` becomes one list, newest first, with a single blank line between it
  and the next heading. No entry's words change.

## Capabilities

None. No behavior and no release content changes, so `skip_specs: true`.

## Impact

- `CHANGELOG.md` only.

## Routing

0. **Does observable behavior change?** No; documentation.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** The changelog rule is machine-checked: the
   `## Unreleased` heading has to exist, which `scripts/check-change.sh` reads on every
   commit. The build and the 186 tests prove nothing else moved.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
