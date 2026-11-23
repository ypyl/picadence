# The screenshots show this build (L0)

## Why

Both images were captured before the pin, the play control, the scrolling window
and the one-row footer existed. A reader comparing them with the panel in front of
them would see two different products.

## What Changes

- `docs/panel.png` and `docs/repeat-ping.png` are replaced with captures of this
  build, cropped to the panel and rounded to the corners the window manager draws.
- The alt text describes what is now in the images.

## Capabilities

None. Images and prose, so `skip_specs: true`.

## Impact

- `docs/panel.png`, `docs/repeat-ping.png`, `README.md`.

## Routing

0. **Does observable behavior change?** No.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Not the pixels, but the crops are taken from
   a running tray with no fixture, so what is shown is what runs.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
