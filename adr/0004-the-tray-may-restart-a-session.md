# 0004. The tray may restart a session the user kept

- Status: Accepted
- Date: 2026-11-22

## Context

The tray reports on sessions whose process is alive. Closing a terminal tab ends
that process, so the session leaves the tray even though pi has already written the
conversation to disk. A tab closed by mistake, or closed at the end of a day and
wanted back the next morning, has to be rebuilt by hand: find the folder, open a
tab there, and pick the right session out of a directory of transcript files whose
names are timestamps and identifiers.

Everything needed to restore it is already on the machine. The tray knows the
session id and the folder from the state file the extension writes. pi keeps one
`.jsonl` file per session under `%USERPROFILE%\.pi\agent\sessions`. Windows
Terminal accepts a new tab with a title, a starting directory and a command:

```
wt.exe -w 0 new-tab --title "<name>" --startingDirectory "<folder>" -- pi --session "<transcript>"
```

What stops this being a small feature is the product's own definition. `AGENTS.md`
says the tray "only reports: it never sends input to pi, starts a tab, or reaches
the network", and the durable spec says it SHALL NOT "start or stop" a session,
with a scenario literally named "the tray never starts a session". This is the
first time the tray would run a program, so it is a decision about what the product
is, not a decision about a menu item.

## Decision

We will let the user keep a session. Keeping one leaves a row in the panel after
its process has ended, until the user stops keeping it. For a kept session that is
not running, the row offers a restart, which opens one new Windows Terminal tab in
that session's folder running `pi --session <the transcript found when it was
kept>`, and does nothing else.

Four constraints are part of the decision, not implementation detail:

1. The tray starts exactly one program, `wt.exe`, with one command line, for a
   session the user kept. There is no general "run a command" path, no shell, and
   nothing starts without a click.
2. A restart is offered only when it can work: the session is not running, its
   conversation is still on disk, and Windows Terminal is installed. Otherwise the
   row says which of those is missing and offers no restart.
3. The command line is built from validated parts and never from text as given: an
   id that is a plain stem, a folder that is an existing directory, a conversation
   that is an existing `.jsonl` file inside pi's sessions directory. A pin that
   does not meet that fails closed and says so.
4. The tray still never sends input to a running session, never stops one, and
   never writes into pi's storage. Its writes remain: state it removes from a dead
   process, a ping file for an armed cadence, a schedule record, and the pin the
   user asked for.

## Consequences

- The "What this is" sentence in `AGENTS.md` changes, and so does the requirement
  that forbade starting a process. Both land with this change, because a product
  whose definition contradicts its behaviour has no owner.
- The tray gains a validation path where before it had only a parser. A pin file is
  the first input whose contents decide what gets executed, so a corrupt or hostile
  one must fail closed. The analysis is recorded with the change.
- A restarted session keeps its id, so the tab mark, the tray row and an armed
  cadence all return by themselves. Restarting a session that is already running
  would give two processes one conversation, so a running session offers no
  restart. Leaving that to the user to avoid is not a safeguard, it is a trap.
- A tab closed mid-turn loses that turn. pi writes the conversation per completed
  turn, so a restart returns everything up to the last one.
- The pin is a new file under `%LOCALAPPDATA%\picadence`, beside the others, with
  the same lifecycle: written when the user keeps a session, deleted when they stop
  keeping it, never pruned behind their back.
- Restoring a whole window, with several tabs and their positions, is out of scope.
  One click, one tab, in the window last used.

## Alternatives

- **A shell alias or a script.** Cheapest of all, no code, and it is what a person
  would do by hand. Rejected because the case that hurts is the session nobody
  remembers the folder and file of, which is exactly the lookup the tray already
  has the inputs for.
- **`--session-id <id>` with the starting directory.** A shorter command line.
  Rejected: pi creates a session when the id is not found, so a stale pin would
  open an empty tab that looks like a restore. The stored path fails loudly.
- **A pi extension that opens the tab.** Rejected: an extension lives inside a
  session, and the case under discussion is a session whose process has ended.
  Nothing is running to do it.
- **`cmd /c start` instead of Windows Terminal.** Rejected: that opens a console
  window running pi, not a tab, and a tab is the whole subject.
- **The tray hosts pi in its own window.** Rejected: it would mean owning a
  terminal, a scrollback buffer and a shell. Windows Terminal does that better and
  is already installed.
- **Remember the last N ended sessions automatically.** Rejected: a list that grows
  unasked turns the panel into a history browser, and saying "keep this one" costs
  one click.
