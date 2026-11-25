# The click that closes holds (L1)

## Why

The panel hides itself when it loses the focus, and the icon click that closes it comes
in two messages: the shell takes the focus first, then the tray's mouse-up arrives. If
those arrive in that order, the panel has already hidden itself when the click is
handled, so the click reads as "the panel is closed" and opens it again. A second click
on the icon is the commonest way to close the panel, and the spec says it closes.

I could not confirm the order on a running tray; that needs a click, which an agent
cannot make. What is certain from the code is that nothing guards it: `PanelController`
decides from `IsOpen` alone, so the outcome depends entirely on which message Windows
sends first. This change makes the close hold whichever way that goes.

## What Changes

- The panel records the moment it hid itself because the focus went elsewhere.
- An icon click that arrives within 400 ms of that is the same gesture as the click that
  closed the panel, so it leaves the panel closed instead of reopening it.
- `PanelController.After` takes that as an argument, so the rule stays a plain function
  with a test, the way the other three panel rules are.
- A fresh open clears the record.

## Capabilities

None. No requirement moves: a second click on the icon closes the panel, which is
already required. So `skip_specs: true`.

## Routing

0. **Does observable behavior change?** Only in the sequence above, where the panel
   currently reopens instead of closing. That is what the requirement already promises.
1. **Can it be undone easily?** Yes, one commit, nothing stored.
2. **Can a machine prove it correct?** No. The rule is testable, and the test is added,
   but whether Windows deactivates the panel before the tray message is a property of
   the shell that only a click can show. That is why this is L1 and not L0.
3. **Does anything outside the change depend on it?** No.

Level: **L1**, `skip_specs: true`: the gate is the whole change.

## What to weigh

The guard is inert unless the panel hid itself from focus loss less than 400 ms before
the click. If the panel already closes correctly on a second click, nothing changes; if
it does not, this is what fixes it. The cost is that a deliberate close-then-reopen in
under 400 ms takes one more click. Two hundred milliseconds would be enough for the
message order and less forgiving of a fast hand; 400 is the cautious end.

## How to check it by hand

Start the tray, click the icon, click it again. The panel should be gone, not blinking
and not still there. Then open it and click another window first: the panel closes as it
does today.

## Approval

- Gate: L1 whole change (skip_specs)
- Approved by: Yauhen Pyl
- Approved at: 2026-11-25
- Artifact: openspec/changes/the-click-that-closes-holds/proposal.md sha256:515bd9c503d6a885a4f29ca671ff25061812ab49677e52c81c55b7492c6cb964
- Words: "(none)"
