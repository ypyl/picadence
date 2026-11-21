# Design

## The two channels

pi exposes one terminal channel to an extension: `ctx.ui.setTitle()`. Its own
examples use it for a spinner, and it is a supported call, not a raw write. That
is the channel this change uses.

The second channel, the progress ring on the tab and on the taskbar, is the ConEmu
`OSC 9;4` sequence, understood by Windows Terminal 1.6 and later:

| state | sequence | what a person sees |
|---|---|---|
| working | `ESC ] 9 ; 4 ; 3 ; 0 BEL` | an indeterminate ring |
| waiting | `ESC ] 9 ; 4 ; 4 ; 100 BEL` | a warning ring, amber |
| failed | `ESC ] 9 ; 4 ; 2 ; 100 BEL` | an error ring, red |
| done, idle | `ESC ] 9 ; 4 ; 0 ; 0 BEL` | no ring |

pi has no API for it. Writing it means `process.stdout.write` while pi owns an
alternate screen, which is very likely harmless, because an operating-system
command does not touch the character grid, and entirely unproven, because nobody
has watched a session with it on. So it ships behind `PICADENCE_TAB_PROGRESS=1`
and off. Turning it on by default is a follow-up that needs a real session and a
pair of eyes.

## The marks

One character, then the label. Colour never carries meaning alone, and a tab title
is plain text anyway, so the mark has to be readable, distinguishable, and present
in the fonts a terminal already uses:

| state | mark | why |
|---|---|---|
| working | `●` | filled, present in most symbol fonts |
| blocked | `▲` | a warning shape, not a colour |
| error | `✖` | the same cross the panel's failed dot sits beside |
| done | `✓` | finished, and different from working at a glance |
| idle | none | a quiet tab should look like a plain tab |

Idle is the one state with no mark. A session that is doing nothing should not
spend the tab's first character saying so, which is the same reason the panel
leaves a quiet row unmarked.

## The label

`name` when the session has one, `folder` otherwise, which is what `describe()`
already computes for the state file. The mark is added to it, never instead of it:
a tab that shows only a glyph identifies nothing.

## Where it is written

`flush()` in `extensions/picadence.js` already runs on every state transition with
the context in hand, and already computes the label. The title is one more line
there, inside the same `try` that keeps a reporting failure away from the session.

A console run (`pi -p`, RPC) never calls it, because `reporting` stays false for
every mode except `tui`, which is the rule the state file already follows.

## Two things Windows Terminal does that we cannot

- **A manual rename wins.** Rename a tab by hand and Terminal stops following the
  title the process sets. Our transitions will try again at the next state change,
  and Terminal may or may not accept it; it is Terminal's rule, not ours.
- **The shell shares the title.** A prompt that sets its own title can overwrite
  ours between turns. Writing on every transition keeps ours fresh, not
  instantaneous.

Both go in the README rather than being discovered.

## Tests

- `tabTitle(state, label)` as a pure function: one case per state, plus the idle
  case with no mark, plus the unnamed case falling back to the folder.
- `tabProgressSequence(state)` as a pure function: the exact escape sequences, so
  a typo cannot reach a terminal.
- The extension test that already drives a fake pi: the title is set on a
  transition, and is not set for a console session.
