# Design

## The decision to turn it on

Three inputs, in order of authority:

```text
PICADENCE_TAB_PROGRESS=0   off, whatever the terminal is
PICADENCE_TAB_PROGRESS=1   on, whatever the terminal is
otherwise                  on when the environment names Windows Terminal
```

The environment Windows Terminal sets is `WT_SESSION`, and some setups also set
`TERM_PROGRAM`. Either one is enough. A multiplexer inside Terminal still carries
`WT_SESSION` into the child process, and tmux swallows the sequence, so the ring
simply does nothing there, which is the same outcome as a terminal that cannot
show it.

The detection lives in one exported function so the rule is a test rather than a
side effect of a live shell:

```js
export function wantsTabProgress(env = process.env) { ... }
```

## Why default-on is now the right call

The reason it was off was that a raw write to the terminal while pi owns the
screen was unproven. Two things changed: the sequence is a standard one that an
operating-system command carries out of band, so it cannot disturb the character
grid, and the tab mark proved the extension can reach the terminal chrome on a
real session. The remaining risk is a visual surprise in a terminal we did not
test, and the switch is one environment variable away.

On by default for Windows Terminal and nowhere else also keeps the product's
promise that it does nothing surprising on a platform it does not claim.

## One collision worth knowing

pi has its own switch, `terminal.showTerminalProgress`, which emits the same
`OSC 9;4` during a run to show a percentage. If both are on, they take turns
overwriting each other. Ours is a state, pi's is progress, and the README says
which one to leave off.

## The changelog

Two entries, in the words of what a person can now see: the tab says what its
session is doing, and Windows Terminal draws it on the tab and the taskbar with
one variable to stop it. Both belong under the version that ships them.

## Tests

- `wantsTabProgress({})` is false.
- `wantsTabProgress({ WT_SESSION: "x" })` is true, and so is `TERM_PROGRAM`.
- `=0` is false even with `WT_SESSION`, and `=1` is true even without it.
- The existing sequence tests keep the characters honest.
- The extension test drives a transition with the switch on and asserts the bytes
  reach the stream, and with it off asserts they do not.
