# The README shows the product (L0)

## Why

The repository is public now and `README.md` asks a reader to imagine a tray
panel from a table of colours. The download link on the release page lands on the
same page. One picture answers "what is this" faster than the paragraph under it.

## What Changes

- Two captures in `docs/`: the panel with three sessions in three states, and the
  same panel with the cadence list open on a row.
- `README.md` shows both, with alt text that says what is in the image, and keeps
  the text as it is.

The captures are of the real panel, driven through its own window messages with
synthetic sessions so no real folder name from this machine is published.

No behavior changes and no requirement moves.

## Capabilities

None. Documentation, so `skip_specs: true`.

## Impact

- `README.md`, `docs/panel.png`, `docs/repeat-ping.png`.

## Routing

0. **Does observable behavior change?** No.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the images are in the tree and the
   README points at paths that exist, which `scripts/verify.sh` keeps honest by
   refusing a broken tree; the layout they show is what the panel tests assert.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
