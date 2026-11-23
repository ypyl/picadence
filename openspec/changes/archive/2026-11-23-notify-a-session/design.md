# Design

## One sequence, four states

```text
OSC 777 ; notify ; picadence ; <folder> needs you   BEL   waiting for the user
OSC 777 ; notify ; picadence ; <folder> finished    BEL   a run ended
OSC 777 ; notify ; picadence ; <folder> failed      BEL   a run failed
```

`\u001b]777;notify;title;body\u0007`, the form #20012 validates. BEL terminates it
because every other sequence this extension writes ends that way, and Windows
Terminal accepts both BEL and ST.

Idle, working, and the `done`-then-idle transition write nothing. The point is the
moment that wants a person, not a heartbeat.

## Why the tray is not involved

Focus is the tempting part and it is the part Microsoft declined to give an
external process. The extension is not an external process here: it is the program
that owns the tab, asking the terminal to tell the user something. The difference
is who decides. Terminal decides whether to show it, the user decides to click it,
and picadence never learns what happened.

## The words

The title and the body are the two fields the sequence carries, separated by `;`.
A folder name containing `;` would become a third field, so the separator, control
characters, and the escape are stripped from both fields before they are written.
A name that is all separators falls back to the session name, then to `session`.

Body wording follows the panel's voice: `trayclaw needs you`, `trayclaw finished`,
`trayclaw failed`.

## The switch

`PICADENCE_NOTIFY=0` in the environment pi starts in turns it off, matching
`PICADENCE_TAB_PROGRESS`. Anything else, including unset, leaves it on, because a
terminal that ignores the sequence costs nothing and a user who upgrades Terminal
should not have to find a variable to get the feature.

## Tests

| behaviour | test |
|---|---|
| a blocked, finished, or failed session writes the sequence | `picadence.test.mjs`, one assertion per state |
| idle and working write nothing | same file |
| `PICADENCE_NOTIFY=0` writes nothing | same file, with the variable set for the call |
| a folder with `;` or an escape cannot add a field | same file, asserting the exact bytes |
| the title and body are never empty | same file |

The terminal's own behaviour is not ours to test: whether it shows a toast, and
whether the click selects the tab or only the window, is #20012's promise and
1.26's business.
