# The cadence list says what it is (L0)

## Why

Right-clicking a session row opens six bare words: `every 15 minutes` through
`off`. Nothing on the list says it is a repeating ping for that session, so the
first time it is opened the choice reads as a mystery rather than a decision.

## What Changes

- The list gains a heading, `Repeat ping`, above the five cadences and off, with
  a one-pixel rule under it.
- The heading is a label, not a choice: a click on it closes the list and arms
  nothing, the same as a click in the list's padding.

No behavior changes. The requirement `A session can be armed with a cadence`
stays exactly as it is: the same five cadences and off, on the same rows, doing
the same thing.

## Capabilities

None. Copy and layout, so `skip_specs: true`.

## Impact

- `src/Piclaw/PanelModel.cs`, `src/Piclaw/Panel.cs`, `DESIGN.md`, and the layout
  test that holds them together.

## Routing

0. **Does observable behavior change?** No. A heading is restyling and naming,
   and every requirement is as it was.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: a layout test asserts the heading
   exists, sits inside the list, and does not overlap a cadence, and the contrast
   test covers the pair it is drawn with.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
